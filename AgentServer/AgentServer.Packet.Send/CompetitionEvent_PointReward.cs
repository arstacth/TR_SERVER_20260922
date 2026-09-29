using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_PointReward : NetPacket
	{
		public CompetitionEvent_PointReward(CompetitionEventHandle.eCompetitionEventResult result, short rewardLevel, bool bUseSeasonPass, List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_POINT_REWARD_ACK);
			ns.Write((int)result);
			ns.Write(rewardLevel);
			ns.Write(bUseSeasonPass);
			if (result == CompetitionEventHandle.eCompetitionEventResult.eCompetitionEventResult_OK)
			{
				ns.Write(exinfo.Count);
				foreach (ExchangeItemInfo item in exinfo)
				{
					ns.Write(item.type);
					ns.Write(item.id);
					ns.Write(item.count);
					ns.Write(int.MaxValue);
				}
			}
			_ = last;
		}
	}
}
