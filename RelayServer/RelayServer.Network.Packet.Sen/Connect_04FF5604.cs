using System;
using System.Linq;
using System.Net;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RelayServer.Structuring.Opcode;
using Serilog;

namespace RelayServer.Network.Packet.Send
{
	public sealed class Connect_04FF5604 : NetPacket
	{
		public IPEndPoint EchoEndPoint { get; }

		public Connect_04FF5604(IPEndPoint remoteIpEndPoint)
			: this(remoteIpEndPoint, (short)Opcodes.eRelayServer_REGIST_ACK)
		{
		}

		public Connect_04FF5604(IPEndPoint remoteIpEndPoint, short opcode)
			: base(2, 0)
		{
			// Official REGIST_ACK echoes the datagram source (what NAT mapped to).
			// Substituting a different local IP is a lie and did not clear the 20s UI.
			EchoEndPoint = remoteIpEndPoint;
			ns.WriteOP(opcode);
			ns.Write((short)2);
			ushort value = (ushort)EchoEndPoint.Port;
			string addr = EchoEndPoint.Address.MapToIPv4().ToString();
			byte[] buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			byte[] buffer2 = BitConverter.GetBytes(Utility.IPToInt(addr)).Reverse().ToArray();
			ns.Write(buffer, 0, 2);
			ns.Write(buffer2, 0, 4);
			ns.Fill(8);
		}
	}
}
