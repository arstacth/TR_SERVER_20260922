using LocalCommons.Network;
using NetMsg.Room;
using RoomServer.Structuring;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_UpdateFarmFishingReward : NetPacket
	{
		public RM_To_AG_UpdateFarmFishingReward(NormalRoom farmroom)
		{
			ns.WriteOP(RMProtocol.RM_UpdateFarmFishingReward_ACK);
			ns.Write(farmroom.ID);
			ns.Write(farmroom.hasFishingReward);
		}
	}
}
