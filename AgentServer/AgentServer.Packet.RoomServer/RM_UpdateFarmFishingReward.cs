using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_UpdateFarmFishingReward : NetPacket
	{
		public RM_UpdateFarmFishingReward(Account User, bool hasFishingReward, byte last)
		{
			ns.WriteOP(RMProtocol.RM_UpdateFarmFishingReward_REQ);
			ns.Write(User.Session);
			ns.Write(hasFishingReward);
			ns.Write(last);
		}
	}
}
