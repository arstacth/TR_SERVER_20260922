using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HardeningDone : NetPacket
	{
		public HardeningDone(long TR, int StoneNum, int ResultStoneNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_STONE_HARDENING_ACK);
			ns.Write(1);
			ns.Write(0);
			ns.Write(TR);
			ns.Write(StoneNum);
			ns.Write(ResultStoneNum);
			_ = last;
		}
	}
}
