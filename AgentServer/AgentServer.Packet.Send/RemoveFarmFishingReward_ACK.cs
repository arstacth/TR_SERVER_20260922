using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RemoveFarmFishingReward_ACK : NetPacket
	{
		public RemoveFarmFishingReward_ACK(List<UserStorageItemInfo> farmRewardList, byte last, int err = 0)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_REMOVE_FARM_MASTER_REWARD_ACK);
			ns.Write(err);
			if (err == 0)
			{
				ns.Write(farmRewardList.Count);
				foreach (UserStorageItemInfo farmReward in farmRewardList)
				{
					ns.Write(farmReward.uniqueNum);
					ns.Write(farmReward.itemNum);
					ns.Write(0);
				}
			}
			_ = last;
		}
	}
}
