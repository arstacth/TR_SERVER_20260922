using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMyRankInfo_ALL : NetPacket
	{
		public GetMyRankInfo_ALL(eRequestRankKind rankKind, CRankListData rankData, byte last)
			: this(rankKind, 0, rankData, last)
		{
		}

		public GetMyRankInfo_ALL(eRequestRankKind rankKind, byte detailRank, CRankListData rankData, byte last)
		{
			_ = detailRank;
			_ = last;
			ns.WriteOP(Opcodes.eServer_RANK_MY_NICKNAME_ACK);
			if (rankData == null)
			{
				ns.Write(65);
				return;
			}
			ns.Write(0);
			ns.Write((byte)rankKind);
			if (rankKind == eRequestRankKind.eRequestRankKind_NORMAL)
			{
				ns.Write(rankData.m_ranking);
				ns.WriteAnsiFixed_intSize(rankData.m_nickname);
				ns.Write(rankData.m_experienceValue);
				ns.Write(rankData.m_experienceValue2);
			}
			else
			{
				ns.Write(rankData.m_ranking);
				ns.WriteAnsiFixed_intSize(rankData.m_nickname);
				// Collection kinds: int score + int level. Farm/sports: int64 score only
				// (extra level left RemainSize=5 on size-34 farm stubs).
				if ((int)rankKind >= 11 || rankKind == eRequestRankKind.eRequestRankKind_ITEM_COLLECTION)
				{
					// Kind 13 packet was 31 bytes and the client stopped 4 bytes early (RemainSize=4).
					ns.Write(rankData.m_experienceValue);
				}
				else
				{
					ns.Write(rankData.m_experienceValue);
				}
			}
			// Do not Write(last) — client leaves RemainSize=1.
		}
	}
}
