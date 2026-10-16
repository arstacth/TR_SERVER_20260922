using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameOptionGetAck : NetPacket
	{
		public GameOptionGetAck(int option, byte last)
		{
			// Thai wire 780: 3 empty len-prefixed strings + 8 option flag bytes + last.
			// Live ACK size is 17; omitting last left Remain=1. Flags are the little-endian
			// GameOption int (and a trailing 0 int), matching KR/Thai packing.
			ns.Write((ushort)780);
			ns.Write((ushort)0);
			ns.Write((ushort)0);
			ns.Write((ushort)0);
			ns.Write(option);
			ns.Write(0);
			// Size 17 Remain 1 — packed GET_ACK is 16 bytes (no TCP last).
			_ = last;
		}
	}
}
