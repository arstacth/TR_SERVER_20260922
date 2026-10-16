using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistEnchantGrade_Fail : NetPacket
	{
		public AlchemistEnchantGrade_Fail(eAlchemistEnchantGradeFailedReason failReason, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_ENCHANT_GRADE_ACK);
			ns.Write(92);
			ns.Write((int)failReason);
			_ = last;
		}
	}
}
