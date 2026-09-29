using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_ModifyFarmMapInfo : NetPacket
	{
		public RM_ModifyFarmMapInfo(Account User, int FarmUniqueNum, int FarmTypeNum, int count, byte[] data, byte last)
		{
			ns.WriteOP(RMProtocol.RM_ModifyFarmMapInfo_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(FarmUniqueNum);
			ns.Write(FarmTypeNum);
			ns.Write(count);
			ns.Write(data, 0, data.Length);
			ns.Write(last);
		}
	}
}
