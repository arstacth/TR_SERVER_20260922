using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RequestJoinGuildInFoACK : NetPacket
	{
		public RequestJoinGuildInFoACK(GuildInfo info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.REQUEST_JOIN_ACK);
			ns.Write((long)info.guildNum);
			ns.Write(info.memberCount);
			ns.Write(info.memberLimit);
			ns.Write(info.level);
			ns.Write(info.kind);
			ns.Write(info.joinMethod);
			ns.Write(info.joinLimitLevelOver);
			ns.Write(info.joinLimitLevelBelow);
			ns.Write(info.foundationDate);
			ns.WriteAnsiFixed_intSize(info.guildName);
			ns.WriteAnsiFixed_intSize(info.masterName);
			ns.WriteAnsiFixed_intSize(info.message);
			ns.Write(0);
			ns.Write(info.exp);
			ns.Write(info.nextLevelExp);
			ns.Write(info.point);
			ns.Write(0);
			ns.Write(0);
			ns.Write((short)0);
			ns.Write((byte)0);
			ns.Write((short)0);
			_ = last;
		}
	}
}
