using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RemoveFarmFishingReward_RoomUpdate_ACK : NetPacket
	{
		public RemoveFarmFishingReward_RoomUpdate_ACK(List<UserStorageItemInfo> farmRewardList, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FISHING_REMOVE_FARM_MASTER_REWARD_NOTIFY);
			ns.Write(farmRewardList.Count);
			foreach (UserStorageItemInfo farmReward in farmRewardList)
			{
				ns.Write(farmReward.itemNum);
			}
			_ = last;
		}
	}
}
