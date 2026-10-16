using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetFarmFishingReward_ACK : NetPacket
	{
		public GetFarmFishingReward_ACK(bool MyFarm, List<UserStorageItemInfo> farmRewardList, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_FARM_MASTER_REWARD_ACK);
			ns.Write(0);
			ns.Write(MyFarm);
			ns.Write(farmRewardList.Count);
			foreach (UserStorageItemInfo farmReward in farmRewardList)
			{
				if (MyFarm)
				{
					ns.Write(farmReward.uniqueNum);
				}
				ns.Write(farmReward.itemNum);
				if (MyFarm)
				{
					ns.Write(0);
				}
			}
		}
	}
}
