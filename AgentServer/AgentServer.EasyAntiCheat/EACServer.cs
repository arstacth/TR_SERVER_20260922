using System;
using System.Collections.Concurrent;
using AgentServer.EasyAntiCheat.Server;
using AgentServer.EasyAntiCheat.Server.Hydra;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using Serilog;

namespace AgentServer.EasyAntiCheat
{
	public static class EACServer
	{
		public sealed class eServer_EAC_MESSAGE_ACK : NetPacket
		{
			public eServer_EAC_MESSAGE_ACK(byte[] message, int length)
			{
				ns.WriteOP(Opcodes.eServer_EAC_MESSAGE_ACK);
				ns.Write(length);
				ns.Write(message, 0, length);
				ns.Write((byte)1);
			}
		}

		private static ConcurrentDictionary<Client, int> client2connection;

		private static ConcurrentDictionary<int, Client> connection2client;

		private static ConcurrentDictionary<int, ClientStatus> connection2status;

		private static readonly object eaclock;

		private static EasyAntiCheatServer<Client> easyAntiCheat;

		static EACServer()
		{
			eaclock = new object();
			client2connection = new ConcurrentDictionary<Client, int>();
			connection2client = new ConcurrentDictionary<int, Client>();
			connection2status = new ConcurrentDictionary<int, ClientStatus>();
			easyAntiCheat = null;
		}

		public static bool DoStartup()
		{
			if (!ServerSettingHolder.ServerSettings.useEasyAntiCheat)
			{
				return false;
			}
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			Log.Information("EAC - [Info] Init EAC");
			easyAntiCheat = new EasyAntiCheatServer<Client>(HandleClientUpdate, "TalesRunnerServer");
			if (easyAntiCheat != null)
			{
				Log.Information("EAC - [Info] EAC Load OK");
				return true;
			}
			Log.Error("EAC - [Info] EAC Load Failed");
			return false;
		}

		public static void DoShutdown()
		{
			if (ServerSettingHolder.ServerSettings.useEasyAntiCheat)
			{
				client2connection.Clear();
				connection2client.Clear();
				connection2status.Clear();
				if (easyAntiCheat != null)
				{
					easyAntiCheat.Dispose();
					easyAntiCheat = null;
				}
			}
		}

		public static void OnJoinGame(ClientConnection connection)
		{
			if (!ServerSettingHolder.ServerSettings.useEasyAntiCheat)
			{
				return;
			}
			if (easyAntiCheat == null)
			{
				OnAuthenticatedLocal(connection.session);
				OnAuthenticatedRemote(connection.session);
				return;
			}
			lock (eaclock)
			{
				PlayerRegisterFlags flags = ((connection.CurrentAccount.Attribute != 0) ? PlayerRegisterFlags.PlayerRegisterFlagAdmin : PlayerRegisterFlags.PlayerRegisterFlagNone);
				Client client = easyAntiCheat.GenerateCompatibilityClient();
				try
				{
					easyAntiCheat.RegisterClient(client, connection.CurrentAccount.UserID, connection.IP, Guid.NewGuid().ToString(), connection.CurrentAccount.UserID, flags);
					client2connection.TryAdd(client, connection.session);
					connection2client.TryAdd(connection.session, client);
					connection2status.TryAdd(connection.session, ClientStatus.Reserved);
					if (ShouldIgnore(connection.session))
					{
						OnAuthenticatedLocal(connection.session);
						OnAuthenticatedRemote(connection.session);
					}
				}
				catch (Exception ex)
				{
					Log.Error("EAC - [TRServer] RegisterClient Fail - ClientID: {0}, Error: {1}", client.ClientID, ex.ToString());
				}
			}
		}

		public static void OnLeaveGame(int Session)
		{
			if (ServerSettingHolder.ServerSettings.useEasyAntiCheat && easyAntiCheat != null && GetClient(Session, out var client))
			{
				easyAntiCheat.UnregisterClient(client);
				client2connection.TryRemove(client, out var _);
				connection2client.TryRemove(Session, out var _);
				connection2status.TryRemove(Session, out var _);
			}
		}

		public static void OnReceivedEACMessage(ClientConnection Client, PacketReader reader)
		{
			if (ServerSettingHolder.ServerSettings.useEasyAntiCheat)
			{
				Account currentAccount = Client.CurrentAccount;
				Client client;
				if (!connection2client.ContainsKey(Client.session))
				{
					Log.Warning("EAC - network packet from invalid connection: {0}, IP: {1}" + currentAccount.UserID, currentAccount.LastIp);
				}
				else if (GetClient(Client.session, out client))
				{
					int length = reader.ReadLEInt32();
					byte[] array = reader.ReadByteArray(length);
					easyAntiCheat.PushNetworkMessage(client, array, array.Length);
				}
			}
		}

		private static bool ShouldIgnore(int Session)
		{
			return false;
		}

		private static bool GetClient(int Session, out Client client)
		{
			return connection2client.TryGetValue(Session, out client);
		}

		private static bool GetConnection(Client client, out int Session)
		{
			return client2connection.TryGetValue(client, out Session);
		}

		private static void HandleClientUpdate(ClientStatusUpdate<Client> clientStatus)
		{
			Client clientObject = clientStatus.ClientObject;
			if (!GetConnection(clientObject, out var Session))
			{
				Log.Error("EAC - status update for invalid client: {0}", clientObject.ClientID);
			}
			else if (!ShouldIgnore(Session))
			{
				if (clientStatus.RequiresKick)
				{
					string message = clientStatus.Message;
					clientStatus.IsBanned(out var timeBanExpires);
					ClientConnection.CurrentAccounts.TryGetValue(Session, out var value);
					Log.Warning("EAC - [TRServer] Kick Player - UserID : {0}, Message : {1}", value.UserID, message);
					KickGameClientByEACClientID(Session, message, timeBanExpires);
				}
				else if (clientStatus.Status == ClientStatus.ClientAuthenticatedLocal)
				{
					OnAuthenticatedLocal(Session);
					easyAntiCheat.SetClientNetworkState(clientObject, networkActive: false);
				}
				else if (clientStatus.Status == ClientStatus.ClientAuthenticatedRemote)
				{
					OnAuthenticatedRemote(Session);
				}
			}
		}

		public static void DoUpdate()
		{
			if (easyAntiCheat == null)
			{
				return;
			}
			try
			{
				easyAntiCheat.HandleClientUpdates();
				Client clientObject;
				byte[] messageBuffer;
				int messageLength;
				while (easyAntiCheat.PopNetworkMessage(out clientObject, out messageBuffer, out messageLength))
				{
					SendEACMessageToGameClient(clientObject, messageBuffer, messageLength);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on EAC DoUpdate:\r\n{0}", ex.ToString());
			}
		}

		private static void KickGameClientByEACClientID(int Session, string Msg, DateTime? banTime)
		{
			if (ClientConnection.CurrentAccounts.TryGetValue(Session, out var value))
			{
				long bantime = (banTime.HasValue ? Utility.ConvertToTimestamp(banTime.Value) : 0);
				value.Connection.SendAsync(new EAC_DisconnectPacket(Msg, bantime, 16));
				value.Connection.Disconnect(5000);
			}
		}

		private static void SendEACMessageToGameClient(Client client, byte[] message, int messageLength)
		{
			Account value;
			if (!GetConnection(client, out var Session))
			{
				Log.Warning("EAC - network packet for invalid client: {0}", client.ClientID);
			}
			else if (ClientConnection.CurrentAccounts.TryGetValue(Session, out value))
			{
				value.Connection.SendAsync(new eServer_EAC_MESSAGE_ACK(message, messageLength));
			}
		}

		private static void OnAuthenticatedLocal(int Session)
		{
			ClientConnection.CurrentAccounts.TryGetValue(Session, out var value);
			if (value.Connection.authStatus == string.Empty)
			{
				value.Connection.authStatus = "ok";
			}
			connection2status[Session] = ClientStatus.ClientAuthenticatedLocal;
		}

		private static void OnAuthenticatedRemote(int Session)
		{
			connection2status[Session] = ClientStatus.ClientAuthenticatedRemote;
		}
	}
}
