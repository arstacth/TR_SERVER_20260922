using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_ReloadFarmMapInfo : NetPacket
	{
		public RM_ReloadFarmMapInfo(Account User, int FarmUniqueNum, byte last)
		{
			ns.WriteOP(RMProtocol.RM_ReloadFarmMapInfo_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(FarmUniqueNum);
			ns.Write(last);
		}
	}
}
