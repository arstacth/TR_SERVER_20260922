using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RewardResult : NetPacket
	{
		public GameRoom_RewardResult(List<GameRewardResult> rewardresult, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAME_REWARD_INFO);
			ns.Write(1);
			ns.Write(rewardresult.Count);
			foreach (GameRewardResult item in rewardresult)
			{
				ns.Write(item.RewardID);
				ns.Write(0);
			}
			ns.Write(1);
			ns.Write(1002);
			ns.Write(0);
			_ = last;
		}
	}
}
