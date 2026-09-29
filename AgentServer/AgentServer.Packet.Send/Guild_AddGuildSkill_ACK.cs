using System.Collections.Generic;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Guild_AddGuildSkill_ACK : NetPacket
	{
		public Guild_AddGuildSkill_ACK(long guildPoint, byte guildSkillPoint, List<GuildSkillInfo> SkillList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.ADD_GUILD_SKILL_ACK);
			ns.Write(1);
			ns.Write(guildPoint);
			ns.Write(guildSkillPoint);
			ns.Write(SkillList.Count);
			foreach (GuildSkillInfo Skill in SkillList)
			{
				ns.Write(Skill.SkillNum);
				ns.Write(Skill.SkillLevel);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
