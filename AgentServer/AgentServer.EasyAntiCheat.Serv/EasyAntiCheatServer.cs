using System;
using System.Collections.Generic;
using Serilog;

namespace AgentServer.EasyAntiCheat.Server.Hydra
{
	public sealed class EasyAntiCheatServer<TClient> : IDisposable where TClient : IEquatable<TClient>
	{
		public delegate void ClientStatusHandler(ClientStatusUpdate<TClient> clientStatus);

		private ClientStatusHandler _ClientStatusHandler;

		internal readonly IDictionary<TClient, int> clientMap;

		private readonly IDictionary<int, TClient> invClientMap;

		private int clientIDCtr;

		public EasyAntiCheatServer(ClientStatusHandler clientStatusHandler, string serverName)
			: this(clientStatusHandler, new ServerConfiguration(60, serverName), (int?)null)
		{
		}

		public EasyAntiCheatServer(ClientStatusHandler clientStatusHandler, int registerTimeout, string serverName)
			: this(clientStatusHandler, new ServerConfiguration(registerTimeout, serverName), (int?)null)
		{
		}

		internal EasyAntiCheatServer(ClientStatusHandler clientStatusHandler, ServerConfiguration serverConfiguration, int? gameID)
		{
			_ClientStatusHandler = clientStatusHandler;
			clientMap = new Dictionary<TClient, int>();
			invClientMap = new Dictionary<int, TClient>();
			try
			{
				if (!(gameID.HasValue ? NativeModule.InitializeWithGameID(gameID.Value, serverConfiguration) : NativeModule.Initialize(serverConfiguration)))
				{
					throw new Exception("Failed to initialize the native module!");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				throw ex;
			}
		}

		public void Dispose()
		{
			NativeModule.Unload();
		}

		public void HandleClientUpdates()
		{
			NativeModule.ClientUpdate Msg;
			while (NativeModule.GetNextClientUpdate(0, out Msg) != 0)
			{
				if (invClientMap.TryGetValue((int)Msg.ClientObject, out var value))
				{
					ClientStatusHandler clientStatusHandler = _ClientStatusHandler;
					TClient clientObject = value;
					ClientStatus status = Msg.Status;
					DateTime? timeBanExpires = ((Msg.TimeBanExpires != 0L) ? new DateTime?(new EpochTime(Msg.TimeBanExpires)) : null);
					string text = new string(Msg.Message);
					char[] trimChars = new char[1];
					clientStatusHandler(new ClientStatusUpdate<TClient>(clientObject, status, timeBanExpires, text.TrimEnd(trimChars)));
				}
			}
		}

		public void RegisterClient(TClient clientObject, string playerGuid, string playerIP)
		{
			RegisterClient(clientObject, playerGuid, playerIP, null);
		}

		public void RegisterClient(TClient clientObject, string playerGuid, string playerIP, string ownerGuid)
		{
			RegisterClient(clientObject, playerGuid, playerIP, ownerGuid, string.Empty, PlayerRegisterFlags.PlayerRegisterFlagNone);
		}

		public void RegisterClient(TClient clientObject, string playerGuid, string playerIP, string ownerGuid, string playerName, PlayerRegisterFlags flags)
		{
			if (playerGuid == null || playerGuid.Length == 0)
			{
				throw new ArgumentNullException("playerGuid", "playerGuid cannot be empty.");
			}
			if (playerIP == null || playerIP.Length == 0)
			{
				throw new ArgumentNullException("playerIP", "playerIP cannot be empty.");
			}
			if (playerName == null)
			{
				throw new ArgumentNullException("playerName", "playerName cannot be null.");
			}
			int num = ++clientIDCtr;
			clientMap.Add(clientObject, num);
			invClientMap.Add(num, clientObject);
			if (!NativeModule.RegisterClient(num, playerGuid, playerIP, ownerGuid, playerName, flags))
			{
				throw new OutOfMemoryException("RegisterClient was unable to register new client!");
			}
		}

		public Client GenerateCompatibilityClient()
		{
			return new Client(NativeModule.GenerateCompatibilityClientID());
		}

		public void UnregisterClient(TClient clientObject)
		{
			if (clientMap.TryGetValue(clientObject, out var value))
			{
				clientMap.Remove(clientObject);
				invClientMap.Remove(value);
				NativeModule.UnregisterClient(value);
			}
		}

		public bool PopNetworkMessage(out TClient clientObject, out byte[] messageBuffer, out int messageLength)
		{
			clientObject = default(TClient);
			messageBuffer = null;
			messageLength = 0;
			int num = NativeModule.PopNetworkMessage(0, out messageBuffer, out messageLength);
			if (num != 0)
			{
				foreach (KeyValuePair<TClient, int> item in clientMap)
				{
					if (item.Value == num)
					{
						clientObject = item.Key;
						return true;
					}
				}
				return false;
			}
			return false;
		}

		public bool PopNetworkMessage(TClient desiredClient, out byte[] messageBuffer, out int messageLength)
		{
			messageBuffer = null;
			messageLength = 0;
			int value = 0;
			if (desiredClient == null || clientMap.TryGetValue(desiredClient, out value))
			{
				int num = NativeModule.PopNetworkMessage(value, out messageBuffer, out messageLength);
				if (num != 0)
				{
					foreach (KeyValuePair<TClient, int> item in clientMap)
					{
						if (item.Value == num)
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		public void SetMaxAllowedMessageLength(TClient clientObject, int maxMessageLength)
		{
			if (clientMap.TryGetValue(clientObject, out var value))
			{
				NativeModule.SetMaxAllowedMessageLength(value, maxMessageLength);
			}
		}

		public void PushNetworkMessage(TClient clientObject, byte[] messageBuffer, int messageLength)
		{
			if (clientMap.TryGetValue(clientObject, out var value))
			{
				NativeModule.PushNetworkMessage(value, messageBuffer, messageLength);
			}
		}

		public void SetClientNetworkState(TClient clientObject, bool networkActive)
		{
			if (clientMap.TryGetValue(clientObject, out var value))
			{
				NativeModule.SetClientNetworkState(value, networkActive);
			}
		}
	}
}
