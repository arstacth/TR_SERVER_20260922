using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.IO;
using CommunityAgentServer.Network;
using CommunityAgentServer.Packet;
using CommunityAgentServer.Structuring;
using CommunityAgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using Serilog;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;

namespace CommunityAgentServer.Network.Connections
{
	public class ClientConnection : ReceiveActor
	{
		private readonly IActorRef _connection;

		private readonly object DDOS_Lock = new object();

		public static ConcurrentDictionary<string, Account> CurrentAccounts { get; } = new ConcurrentDictionary<string, Account>();


		public static ConcurrentDictionary<string, ConnectionInfo> DDOS_IP { get; } = new ConcurrentDictionary<string, ConnectionInfo>();


		public EndPoint EP { get; set; }

		public string IP { get; private set; }

		public Account CurrentAccount { get; set; }

		public ClientConnection(IActorRef connection, EndPoint remote)
		{
			ClientConnection clientConnection = this;
			UntypedActor.Context.Watch(connection);
			_connection = connection;
			EP = remote;
			IP = ((IPEndPoint)EP).Address.ToString();
			Log.Information("Client: {0} connected", EP);
			Account account2 = (CurrentAccount = new Account
			{
				Connection = this,
				isDisconnected = false
			});
			DDOS_Filter(IP, 1);
			MemoryStream memoryStream = new MemoryStream();
			Receive(delegate(Tcp.Received received)
			{
				int count = received.Data.Count;
				memoryStream.Write(received.Data.ToArray(), 0, count);
				byte[] array = memoryStream.ToArray();
				int num = 0;
				if (count > 0)
				{
					while (true)
					{
						int num2 = 0;
						num2 = ((array.Length - num >= 4) ? BitConverter.ToInt32(array, num) : (-1));
						if (array.Length - num < num2 || num2 == -1)
						{
							break;
						}
						try
						{
							if (num2 < 8)
							{
								CmIntercept.Raw(array, num, num2, "short frame, skip");
								if (num2 <= 0)
								{
									break;
								}
								num += num2;
								continue;
							}
							int num3 = num2 - 4;
							byte[] array2 = new byte[num3];
							Buffer.BlockCopy(array, num + 4, array2, 0, num3);
							clientConnection.HandleReceived(array2);
							num += num2;
						}
						catch (Exception ex)
						{
							Log.Error("Packet Receive Error:{0}, HeadLength:{1}, buffer:{2}", ex.ToString(), num2, Utility.ByteArrayToString(received.Data.ToArray()));
							return;
						}
					}
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = new MemoryStream();
					memoryStream.Write(array, num, array.Length - num);
				}
			});
			Receive<Tcp.ConnectionClosed>(delegate
			{
				try
				{
					clientConnection.CurrentAccount.isDisconnected = true;
					CurrentAccounts.TryRemove(clientConnection.CurrentAccount.NickName, out var _);
				}
				catch
				{
					Log.Warning("Client IP: {0} disconnected,But the remove fail", remote);
				}
				Log.Information("Client IP: {0} disconnected", remote);
				UntypedActor.Context.Stop(clientConnection.Self);
			});
			Receive<Terminated>(delegate
			{
				try
				{
					clientConnection.CurrentAccount.isDisconnected = true;
					CurrentAccounts.TryRemove(clientConnection.CurrentAccount.NickName, out var _);
				}
				catch
				{
					Log.Warning("Client IP: {0} died,But the remove fail", remote);
				}
				Log.Information("Client IP: {0} died", remote);
				UntypedActor.Context.Stop(clientConnection.Self);
			});
		}

		public void SendAsync(NetPacket packet)
		{
			byte[] wire = packet.Compile();
			int op = 0;
			if (wire != null && wire.Length >= 10)
			{
				op = wire[8] | (wire[9] << 8);
			}
			CmIntercept.Server(wire, op, "");
			_connection.Tell(Tcp.Write.Create(ByteString.FromBytes(wire)));
		}

		public void Disconnect()
		{
			_connection.Tell(Tcp.Close.Instance);
		}

		public void HandleReceived(byte[] data)
		{
			try
			{
				if (CurrentAccount == null || CurrentAccount.isDisconnected)
				{
					return;
				}
				PacketReader packetReader = new PacketReader(data, 0);
				packetReader.ReadLEUInt32();
				short num = packetReader.ReadLEInt16();
				CmIntercept.Client(data, num, "");
				switch ((eCommunityAgentOpcode)num)
				{
				case eCommunityAgentOpcode.LOGIN_REQ:
					CommonHandle.Handle_0x02(this, packetReader);
					break;
				case eCommunityAgentOpcode.FORWARD_MULTI_REQ:
					CommonHandle.Handle_0x06(this, packetReader);
					break;
				case eCommunityAgentOpcode.FORWARD_ONE_REQ:
					CommonHandle.Handle_0x07(this, packetReader);
					break;
				case eCommunityAgentOpcode.PING_REQ:
				{
					CommonHandle.Handle_Ping(this);
					DDOS_IP.TryRemove(IP, out var _);
					break;
				}
				case eCommunityAgentOpcode.PROFILE_REQ:
					CommonHandle.Handle_0x0E(this, packetReader);
					break;
				case eCommunityAgentOpcode.CHECK_GIFT_REQ:
					CommonHandle.Handle_CheckGift(this, packetReader);
					break;
				default:
				{
					string text = Utility.ByteArrayToString(packetReader.Buffer);
					if (text.Length > 128)
					{
						text = text.Substring(0, Math.Min(256, text.Length));
						Log.Warning("Unhandle opcode: {0:X2} | {1}, ip:{2}", num, text, IP);
					}
					else
					{
						Log.Warning("Unhandle opcode: {0:X2} | {1}", num, text);
					}
					if (!string.IsNullOrEmpty(CurrentAccount.NickName))
					{
						DDOS_Filter(IP, 2);
					}
					break;
				}
				case eCommunityAgentOpcode.IGNORE_0:
				case eCommunityAgentOpcode.IGNORE_4:
				case eCommunityAgentOpcode.IGNORE_12:
					break;
				}
				packetReader.Clear();
			}
			catch (Exception ex)
			{
				Log.Error("ClientConnection HandleReceived Error:{0}", ex.ToString());
			}
		}

		private void DDOS_Filter(string ip, int type)
		{
			try
			{
				if (!Conf.BlockDDOS)
				{
					return;
				}
				lock (DDOS_Lock)
				{
					if (DDOS_IP.TryGetValue(ip, out var value))
					{
						if (value.UnknownOpcodeTime >= Conf.JudgeTime || (DateTime.Compare(DateTime.Now, value.FirstTimeConnect.AddSeconds(10.0)) <= 0 && value.ConnectTime >= Conf.MaxConnectTime))
						{
							IRule rule = FirewallManager.Instance.Rules.FirstOrDefault((IRule r) => r.Name == "DDOS Block");
							if (rule != null)
							{
								List<IAddress> list = rule.RemoteAddresses.ToList();
								list.Add(SingleIP.FromIPAddress(IPAddress.Parse(ip)));
								rule.RemoteAddresses = list.ToArray();
								Log.Warning("Detected ip:{0} try to DDOS server!", ip);
							}
						}
						switch (type)
						{
						case 1:
							DDOS_IP[ip].ConnectTime++;
							break;
						case 2:
							DDOS_IP[ip].UnknownOpcodeTime++;
							break;
						}
						if (DateTime.Compare(DateTime.Now, value.FirstTimeConnect.AddSeconds(10.0)) > 0)
						{
							DDOS_IP[ip].FirstTimeConnect = DateTime.Now;
							DDOS_IP[ip].UnknownOpcodeTime = ((type == 2) ? 1 : 0);
							DDOS_IP[ip].ConnectTime = ((type == 1) ? 1 : 0);
						}
					}
					else
					{
						DDOS_IP.TryAdd(ip, new ConnectionInfo
						{
							UnknownOpcodeTime = ((type == 2) ? 1 : 0),
							ConnectTime = ((type == 1) ? 1 : 0),
							FirstTimeConnect = DateTime.Now
						});
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on filtering DDOS:\r\n{0}", ex.ToString());
			}
		}
	}
}
