using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 2068. Overpop always failed at offset 35 needing 4 → expected size 39.
	/// User row: byte eventType + point/acc/reward/season as int32 + extra int,
	/// then one trailing int (second-list count). Empty count=0 keeps three ints (size 26).
	/// </summary>
	public sealed class CompetitionEvent_PartyUserInfo : NetPacket
	{
		public CompetitionEvent_PartyUserInfo(int PartyType, int SubPartyType, List<CompetitionPartyUserInfo> CompetitionPartyInfo, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_PARTY_USER_INFO_ACK);
			ns.Write(0);
			ns.Write((short)PartyType);
			ns.Write((short)SubPartyType);
			int n = CompetitionPartyInfo != null ? CompetitionPartyInfo.Count : 0;
			ns.Write(n);
			if (n <= 0)
			{
				// Live empty ACK size 26.
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				return;
			}
			foreach (CompetitionPartyUserInfo item in CompetitionPartyInfo)
			{
				ns.Write(item.eventType);
				ns.Write(item.point);
				ns.Write(item.accPoint);
				ns.Write((int)item.rewardLevel);
				ns.Write((int)item.ReceivedSeasonPassRewardLevel);
				ns.Write(0);
			}
			ns.Write(0);
		}
	}
}
