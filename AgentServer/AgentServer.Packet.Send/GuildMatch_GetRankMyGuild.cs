using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildMatch_GetRankMyGuild : NetPacket
	{
		public GuildMatch_GetRankMyGuild(byte rankKind, GuildMatchRank i, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_RANK_MY_GUILD_ACK);
			ns.Write(rankKind);
			ns.Write(i.rank);
			ns.Write(i.guildNum);
			ns.WriteAnsiFixed_intSize(i.guildName);
			ns.Write(i.win);
			ns.Write(i.lose);
			ns.Write(0L);
			ns.Write((short)(-1));
			ns.Write(i.level);
			ns.Write((short)0);
			_ = last;
		}
	}
}
