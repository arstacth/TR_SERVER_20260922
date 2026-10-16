using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnights_AttackCheck_Unk_Ack : NetPacket
	{
		public TalesKnights_AttackCheck_Unk_Ack(int GroupNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_ADVENTURE_RESULT_ACK);
			ns.Write(3);
			ns.Write(GroupNum);
			ns.Write(57);
			_ = last;
		}
	}
}
