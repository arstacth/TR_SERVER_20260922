using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistDisjoint_Fail : NetPacket
	{
		public AlchemistDisjoint_Fail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_DISJOINT_ACK);
			ns.Write(91);
			_ = last;
		}
	}
}
