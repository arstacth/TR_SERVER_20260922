using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Guild_ResetGuildSkill_ACK : NetPacket
	{
		public Guild_ResetGuildSkill_ACK(long guildPoint, byte guildSkillPoint, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.RESET_GUILD_SKILL_ACK);
			ns.Write(1);
			ns.Write(guildPoint);
			ns.Write(guildSkillPoint);
			ns.Write(0);
			_ = last;
		}
	}
}
