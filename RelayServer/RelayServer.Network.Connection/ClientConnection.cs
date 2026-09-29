using System;
using System.Collections.Concurrent;
using System.Net;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RelayServer.Network.Packet;
using RelayServer.Network;
using RelayServer.Structuring;
using RelayServer.Structuring.Opcode;
using RelayServer;
using Serilog;

namespace RelayServer.Network.Connections
{
	public static class ClientConnection
	{
		public static ConcurrentDictionary<int, AccountInfo> CurrentAccounts { get; } = new ConcurrentDictionary<int, AccountInfo>();

		public static void HandleReceived(byte[] data, EndPoint endPoint)
		{
			try
			{
				if (data == null || data.Length < 10)
				{
					return;
				}
				// 2025 native CRelayServerManager::_dispatchUDPPacket:
				// inbound  len_u32 | crc_u32 | byVer_u8(=4) | opcode_u16 | body
				// outbound len_u32 | crc_u32 | opcode_u16 | body   (no byVer on ACK)
				short opcode;
				PacketReader packetReader;
				if (data.Length >= 11 && data[0] >= 10 && data[1] == 0 && data[2] == 0 && data[3] == 0)
				{
					int wireLen = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
					if (wireLen > 0 && wireLen <= data.Length)
					{
						uint gotCrc = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
						uint wantCrc = Utility.CheckSum(Slice(data, 8, wireLen - 8));
						if (gotCrc != wantCrc)
						{
							Log.Warning("[PACKET]crc fail from {0} got={1:X8} want={2:X8} len={3}", endPoint, gotCrc, wantCrc, wireLen);
						}
					}
					packetReader = new PacketReader(data, 0);
					packetReader.ReadLEInt32();
					packetReader.ReadLEUInt32();
					byte byVer = packetReader.ReadByte();
					if (byVer != 4)
					{
						Log.Warning("!!! Not match Relay Protocol from {0}, byVer : {1}", endPoint, byVer);
					}
					opcode = packetReader.ReadLEInt16();
				}
				else if (data.Length >= 10 && (data[8] | (data[9] << 8)) >= 21 && (data[8] | (data[9] << 8)) <= 26)
				{
					packetReader = new PacketReader(data, 0);
					packetReader.ReadLEInt32();
					packetReader.ReadLEUInt32();
					opcode = packetReader.ReadLEInt16();
				}
				else
				{
					short[] opcodes = TryOpcodes(data);
					if (Conf.ProtocolDebug)
					{
						Log.Information("UDP parse opcodes [{0}] len {1} from {2}", string.Join(",", opcodes), data.Length, endPoint);
					}
					foreach (short tryOp in opcodes)
					{
						if (tryOp != (short)Opcodes.eRelayServer_REGIST_REQ && tryOp != (short)Opcodes.eRelayServer_P2P_WRAP_REQ && tryOp != (short)Opcodes.eRelayServer_LIVE_MSG_REQ && tryOp != (short)Opcodes.eUDPProtocol_PING_REQ && tryOp != (short)Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_REQ && tryOp != (short)Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_ACK)
						{
							continue;
						}
						packetReader = ReaderForOpcode(data, tryOp);
						UdpIntercept.Client(data, endPoint, tryOp, "alt-parse");
						Dispatch(tryOp, packetReader, endPoint);
						return;
					}
					IPEndPoint iPEndPoint = endPoint as IPEndPoint;
					if (Conf.ProtocolDebug)
					{
						Log.Information("Unknown UDP Packet:{0}, ip:{1}", Utility.ByteArrayToString(data), iPEndPoint == null ? "?" : iPEndPoint.Address.ToString());
					}
					UdpIntercept.Client(data, endPoint, -1, "UNKNOWN parse");
					return;
				}
				if (Conf.ProtocolDebug)
				{
					Log.Information("UDP opcode {0} len {1} from {2}", opcode, data.Length, endPoint);
				}
				UdpIntercept.Client(data, endPoint, opcode, "");
				Dispatch(opcode, packetReader, endPoint);
			}
			catch (Exception ex)
			{
				Log.Error("HandleReceived Error:{0}", ex.ToString());
			}
		}

		private static byte[] Slice(byte[] data, int offset, int count)
		{
			if (count < 0)
			{
				count = 0;
			}
			if (offset + count > data.Length)
			{
				count = data.Length - offset;
			}
			if (count < 0)
			{
				count = 0;
			}
			byte[] slice = new byte[count];
			if (count > 0)
			{
				Buffer.BlockCopy(data, offset, slice, 0, count);
			}
			return slice;
		}

		private static void Dispatch(short opcode, PacketReader packetReader, EndPoint endPoint)
		{
			switch ((Opcodes)opcode)
			{
			case Opcodes.eRelayServer_REGIST_REQ:
				UDPHandle.Handle_FirstConnect(packetReader, endPoint);
				return;
			case Opcodes.eRelayServer_LIVE_MSG_REQ:
				UDPHandle.Handle_Ping(packetReader, endPoint);
				return;
			case Opcodes.eRelayServer_P2P_WRAP_REQ:
				UDPHandle.Handle_ConnectUser_04FF5804(packetReader, endPoint);
				return;
			case Opcodes.eUDPProtocol_PING_REQ:
				UDPHandle.Handle_PingReq(packetReader, endPoint);
				return;
			case Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_REQ:
				UDPHandle.Handle_ConfirmReq(packetReader, endPoint);
				return;
			case Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_ACK:
				UDPHandle.Handle_ConfirmAck(packetReader, endPoint);
				return;
			}
			IPEndPoint ep = endPoint as IPEndPoint;
			Log.Information("Unhandled UDP opcode {0} from {1}", opcode, ep == null ? "?" : ep.ToString());
			UdpIntercept.Client(packetReader != null ? packetReader.Buffer : null, endPoint, opcode, "UNHANDLED opcode");
		}

		private static short[] TryOpcodes(byte[] data)
		{
			System.Collections.Generic.List<short> list = new System.Collections.Generic.List<short>();
			if (data.Length >= 2)
			{
				list.Add((short)(data[0] | (data[1] << 8)));
			}
			if (data.Length >= 10)
			{
				list.Add((short)(data[8] | (data[9] << 8)));
			}
			if (data.Length >= 11)
			{
				list.Add((short)(data[9] | (data[10] << 8)));
			}
			return list.ToArray();
		}

		private static PacketReader ReaderForOpcode(byte[] data, short opcode)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			if (data.Length >= 11 && (short)(data[9] | (data[10] << 8)) == opcode)
			{
				packetReader.ReadLEInt32();
				packetReader.ReadLEUInt32();
				packetReader.Offset++;
				packetReader.ReadLEInt16();
			}
			else if (data.Length >= 10 && (short)(data[8] | (data[9] << 8)) == opcode)
			{
				packetReader.ReadLEInt32();
				packetReader.ReadLEUInt32();
				packetReader.ReadLEInt16();
			}
			else
			{
				packetReader.ReadLEInt16();
			}
			return packetReader;
		}
	}
}
