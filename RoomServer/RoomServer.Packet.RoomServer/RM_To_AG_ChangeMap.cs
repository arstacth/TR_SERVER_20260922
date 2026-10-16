using LocalCommons.Network;
using NetMsg.Room;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_ChangeMap : NetPacket
	{
		public RM_To_AG_ChangeMap(int roomid, int MapNum)
		{
			ns.WriteOP(RMProtocol.RM_ChangeMap_REQ);
			ns.Write(roomid);
			ns.Write(MapNum);
		}
	}
}
