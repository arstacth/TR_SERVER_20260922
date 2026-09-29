using System;
using System.Net;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RelayServer;
using RelayServer.Network.Connections;
using RelayServer.Network.Packet.Send;
using RelayServer.Structuring;
using Serilog;

namespace RelayServer.Network.Packet
{
	public class UDPHandle
	{
		public static void Handle_FirstConnect(PacketReader reader, EndPoint endPoint)
		{
			int num = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			IPEndPoint iPEndPoint = endPoint as IPEndPoint;
			ushort uDPPort = (ushort)iPEndPoint.Port;
			string iP = iPEndPoint.Address.MapToIPv4().ToString();
			AccountInfo accountInfo = new AccountInfo
			{
				Session = num,
				UDPPort = uDPPort,
				IP = iP,
				LastPingTime = Utility.CurrentTimeMilliseconds(),
				remoteIpEndPoint = iPEndPoint
			};
			ClientConnection.CurrentAccounts.AddOrUpdate(num, accountInfo, (int _, AccountInfo __) => accountInfo);
			Connect_04FF5604 ackPkt = new Connect_04FF5604(iPEndPoint);
			byte[] ack = ackPkt.Compile();
			RawUdpHost.Send(ack, endPoint);
			if (Conf.ProtocolDebug)
			{
				Log.Information("Send eRelayServer_REGIST_ACK to [{0}] session={1} bytes={2} {3}", iPEndPoint, num, ack.Length, BitConverter.ToString(ack));
			}
			// Official lobby: REGIST_ACK only. Extra CONFIRM/LIVE here made UDPSTAT
			// Recv=(4,3) vs Sent=(2,2) and the 20s ping check still fired.
		}

		public static void Handle_PingReq(PacketReader reader, EndPoint endPoint)
		{
			int session = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			if (reader.Remaining >= 1)
			{
				reader.ReadByte();
			}
			int tick = reader.Remaining >= 4 ? reader.ReadLEInt32() : Environment.TickCount;
			byte[] ack = new Connect_PingAck(session, tick).Compile();
			RawUdpHost.Send(ack, endPoint);
			if (Conf.ProtocolDebug)
			{
				Log.Information("Send eUDPProtocol_PING_ACK to [{0}] session={1} bytes={2} {3}", endPoint, session, ack.Length, BitConverter.ToString(ack));
			}
		}

		public static void Handle_ConfirmReq(PacketReader reader, EndPoint endPoint)
		{
			int session = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			byte[] ack = new Connect_ConfirmAck(session).Compile();
			RawUdpHost.Send(ack, endPoint);
			if (Conf.ProtocolDebug)
			{
				Log.Information("Send eUDPProtocol_FOR_CONNECT_CONFIRM_ACK to [{0}] session={1} bytes={2} {3}", endPoint, session, ack.Length, BitConverter.ToString(ack));
			}
		}

		public static void Handle_ConfirmAck(PacketReader reader, EndPoint endPoint)
		{
			int session = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			if (Conf.ProtocolDebug)
			{
				Log.Information("Recv eUDPProtocol_FOR_CONNECT_CONFIRM_ACK from [{0}] session={1}", endPoint, session);
			}
		}

		public static void Handle_Ping(PacketReader reader, EndPoint endPoint)
		{
			int key = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			IPEndPoint ep = endPoint as IPEndPoint;
			if (ep != null && ClientConnection.CurrentAccounts.TryGetValue(key, out var value))
			{
				value.LastPingTime = Utility.CurrentTimeMilliseconds();
				value.remoteIpEndPoint = ep;
			}
			else if (ep != null)
			{
				AccountInfo created = new AccountInfo
				{
					Session = key,
					UDPPort = (ushort)ep.Port,
					IP = ep.Address.MapToIPv4().ToString(),
					LastPingTime = Utility.CurrentTimeMilliseconds(),
					remoteIpEndPoint = ep
				};
				ClientConnection.CurrentAccounts.TryAdd(key, created);
			}
			// Official LIVE_ACK is 10B (len+crc+op26), no byVer. Must always send;
			// missing ACK is the 20s lobby watchdog after REGIST succeeds.
			byte[] ack = new Connect_04FF5A04().Compile();
			if (ack == null || ack.Length != 10 || ack[8] != 0x1A)
			{
				ack = new byte[] { 10, 0, 0, 0, 0x24, 0xE8, 0xF4, 0xF1, 0x1A, 0 };
			}
			RawUdpHost.Send(ack, endPoint);
			if (Conf.ProtocolDebug)
			{
				Log.Information("Send eRelayServer_LIVE_MSG_ACK to [{0}] session={1} bytes={2} {3}", endPoint, key, ack.Length, BitConverter.ToString(ack));
			}
		}

		public static void Handle_ConnectUser_04FF5804(PacketReader reader, EndPoint endPoint)
		{
			try
			{
				int key = reader.ReadLEInt32();
				short length = reader.ReadLEInt16();
				byte[] array = reader.ReadByteArray(length);
				if (ClientConnection.CurrentAccounts.TryGetValue(key, out var value))
				{
					value.SendAsync(new Connect_04FF5804(array), value.remoteIpEndPoint);
				}
			}
			catch (Exception ex)
			{
				Log.Error("ConnectUser Error:{0}, buffer:{1}", ex.ToString(), Utility.ByteArrayToString(reader.Buffer));
			}
		}
	}
}
