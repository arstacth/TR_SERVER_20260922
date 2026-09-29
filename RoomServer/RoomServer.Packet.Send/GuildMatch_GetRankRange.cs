using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Guild;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GuildMatch_GetRankRange : NetPacket
	{
		public GuildMatch_GetRankRange(byte rankKind, List<GuildMatchRank> GuildMatchRankList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_RANK_ACK);
			ns.Write((short)0);
			ns.Write(rankKind);
			ns.Write(GuildMatchRankList.Count);
			foreach (GuildMatchRank GuildMatchRank in GuildMatchRankList)
			{
				ns.Write(GuildMatchRank.rank);
				ns.Write(GuildMatchRank.guildNum);
				ns.WriteAnsiFixed_intSize(GuildMatchRank.guildName);
				ns.Write(GuildMatchRank.win);
				ns.Write(GuildMatchRank.lose);
				ns.Write(0L);
				ns.Write((short)(-1));
				ns.Write(GuildMatchRank.level);
				ns.Write((short)0);
			}
			_ = last;
		}
	}
}
