using System.Collections.Generic;
using AgentServer.Holders;
using AgentServer.Structuring.HotTime;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HotTimeInfos : NetPacket
	{
		public HotTimeInfos(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_HOTTIME_INFO_ACK);
			ns.Write(HotTimeHolder.HotTimeInfos.Count);
			foreach (KeyValuePair<long, HotTimeInfo> hotTimeInfo in HotTimeHolder.HotTimeInfos)
			{
				ns.Write(hotTimeInfo.Key);
				ns.Write(hotTimeInfo.Value.HotTimeType);
				ns.Write(hotTimeInfo.Value.HotTimeImportant);
				ns.Write(hotTimeInfo.Value.StartTime);
				ns.Write(hotTimeInfo.Value.FinishTime);
				ns.Write(hotTimeInfo.Value.RequiredTime);
				ns.Write(hotTimeInfo.Value.LimitedUserNum);
				ns.Write(hotTimeInfo.Value.RewardKind);
				ns.Write(hotTimeInfo.Value.RewardInfos.Count);
				foreach (HotTimeRewardInfo rewardInfo in hotTimeInfo.Value.RewardInfos)
				{
					ns.Write(rewardInfo.RewardType);
					ns.Write(rewardInfo.RewardValue);
				}
			}
			_ = last;
		}
	}
}
