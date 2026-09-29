using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyMemberGradeACK : NetPacket
	{
		public ModifyMemberGradeACK(string Name, short Grade, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MODIFY_MEMBER_GRADE_ACK);
			ns.WriteAnsiFixed_intSize(Name);
			ns.Write(Grade);
			_ = last;
		}
	}
}
