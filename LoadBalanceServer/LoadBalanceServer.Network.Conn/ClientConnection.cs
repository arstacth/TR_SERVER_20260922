using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.IO;
using LoadBalanceServer.Packet;
using LoadBalanceServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using Serilog;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;

namespace LoadBalanceServer.Network.Connections
{
	public class ClientConnection : ReceiveActor
	{
		private readonly IActorRef _connection;

		private readonly object DDOS_Lock = new object();

		public static ConcurrentDictionary<string, ConnectionInfo> DDOS_IP { get; } = new ConcurrentDictionary<string, ConnectionInfo>();


		public EndPoint IP { get; set; }

		public ClientConnection(IActorRef connection, EndPoint remote)
		{
			ClientConnection clientConnection = this;
			UntypedActor.Context.Watch(base.Self);
			_connection = connection;
			IP = remote;
			Log.Information("Client: {0} connected", IP);
			DDOS_Filter(((IPEndPoint)IP).Address.ToString(), 1);
			Receive(delegate(Tcp.Received received)
			{
				PacketReader packetReader = new PacketReader(received.Data.ToArray(), 0);
				int size = packetReader.Size;
				int num = packetReader.ReadLEInt32() - 4;
				int num2 = 4;
				if (size > 0 && size >= num + num2)
				{
					byte[] array = new byte[num];
					Buffer.BlockCopy(packetReader.Buffer, num2, array, 0, num);
					clientConnection.HandleReceived(array);
					packetReader.Clear();
				}
			});
			Receive<Tcp.ConnectionClosed>(delegate
			{
				Log.Information("Client: {0} disconnected", remote);
				UntypedActor.Context.Stop(clientConnection.Self);
			});
			Receive<Terminated>(delegate
			{
				Log.Information("Client: {0} died", remote);
				UntypedActor.Context.Stop(clientConnection.Self);
			});
		}

		public void SendAsync(NetPacket packet)
		{
			_connection.Tell(Tcp.Write.Create(ByteString.FromBytes(packet.Compile())));
		}

		public void Disconnect()
		{
			_connection.Tell(Tcp.Close.Instance);
		}

		public void HandleReceived(byte[] data)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			packetReader.ReadLEUInt32();
			if (packetReader.ReadLEInt16() == (short)LBSOpcodes.HASH_CHECK_REQ)
			{
				CommonHandle.Handle_0x01(this, packetReader);
				return;
			}
			string text = Utility.ByteArrayToString(packetReader.Buffer);
			if (text.Length > 128)
			{
				text = text.Substring(0, Math.Min(256, text.Length));
				Log.Information("Unhandle opcode: {0}, ip:{1}", text, this);
			}
			else
			{
				Log.Information("Unhandle opcode: {0}", text);
			}
			DDOS_Filter(((IPEndPoint)IP).Address.ToString(), 2);
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
