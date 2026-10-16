using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class BonusStage_RewardList : NetPacket
	{
		public BonusStage_RewardList(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BONUS_STAGE_REWARD_INFO_LIST_ACK);
			ns.Write(0);
			ns.Write(room.BonusStageRewardInfo.Count);
			foreach (KeyValuePair<int, GameRewardResult> item in room.BonusStageRewardInfo.OrderBy((KeyValuePair<int, GameRewardResult> o) => o.Key))
			{
				ns.Write(item.Key);
				ns.Write(item.Value.RewardID);
			}
			_ = last;
		}
	}
}
