using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Guild_LevelUP_ACK : NetPacket
	{
		public Guild_LevelUP_ACK(GuildInfo info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.LEVEL_UP_ACK);
			ns.Write((short)info.level);
			ns.Write(info.exp);
			ns.Write(info.point);
			ns.Write(info.memberLimit);
			ns.Write(info.nextLevelExp);
			ns.Write(info.SkillPoint);
			_ = last;
		}
	}
}
