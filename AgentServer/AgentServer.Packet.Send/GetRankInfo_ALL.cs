using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetRankInfo_ALL : NetPacket
	{
		public GetRankInfo_ALL(eRequestRankKind rankKind, List<CRankListData> ranklist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_RANK_ACK);
			ns.Write((byte)rankKind);
			ns.Write((byte)ranklist.Count);
			if (rankKind == eRequestRankKind.eRequestRankKind_NORMAL)
			{
				foreach (CRankListData item in ranklist)
				{
					ns.Write(item.m_ranking);
					ns.WriteAnsiFixed_intSize(item.m_nickname);
					ns.Write(item.m_experienceValue);
					ns.Write(item.m_experienceValue2);
					ns.Write((item.m_ranking < 100) ? item.m_ranking : 0);
				}
			}
			else
			{
				foreach (CRankListData item2 in ranklist)
				{
					ns.Write(item2.m_ranking);
					ns.WriteAnsiFixed_intSize(item2.m_nickname);
					// Collection kind 11+/13: int score + int userLevel (+ pad), not int64.
					// int64 + level put level in the wrong slot → huge EXP / wrong crown.
					if ((int)rankKind >= 11 || rankKind == eRequestRankKind.eRequestRankKind_ITEM_COLLECTION)
					{
						// Client reads score as int64. int+level packed to 27<<32|point (115964285855).
						ns.Write(item2.m_experienceValue);
						ns.Write(0);
					}
					else
					{
						ns.Write(item2.m_experienceValue);
						ns.Write(item2.m_level);
					}
				}
			}
			_ = last;
		}
	}
}
