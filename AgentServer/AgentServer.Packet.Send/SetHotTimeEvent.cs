using AgentServer.Structuring.HotTime;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SetHotTimeEvent : NetPacket
	{
		public SetHotTimeEvent(long hottimeid, byte isUpdate, HotTimeInfo info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_HOTTIME_EVENT_SETTING_ACK);
			ns.Write((byte)1);
			ns.Write(hottimeid);
			ns.Write(info.HotTimeType);
			ns.Write(info.HotTimeImportant);
			ns.Write(info.StartTime);
			ns.Write(info.FinishTime);
			ns.Write(info.RequiredTime);
			ns.Write(info.LimitedUserNum);
			ns.Write(info.RewardKind);
			ns.Write(info.RewardInfos.Count);
			foreach (HotTimeRewardInfo rewardInfo in info.RewardInfos)
			{
				ns.Write(rewardInfo.RewardType);
				ns.Write(rewardInfo.RewardValue);
			}
			_ = last;
		}
	}
}
