using LocalCommons.Network;

namespace RelayServer.Network.Packet.Send
{
	public sealed class Connect_UdpOpcode : NetPacket
	{
		public Connect_UdpOpcode(short opcode)
			: this(opcode, null)
		{
		}

		public Connect_UdpOpcode(short opcode, int? extra)
			: base(2, 0)
		{
			ns.WriteOP(opcode);
			if (extra.HasValue)
			{
				ns.Write(extra.Value);
			}
		}
	}
}
