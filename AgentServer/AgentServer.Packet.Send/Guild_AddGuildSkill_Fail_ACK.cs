using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Guild_AddGuildSkill_Fail_ACK : NetPacket
	{
		public Guild_AddGuildSkill_Fail_ACK(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.ADD_GUILD_SKILL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
