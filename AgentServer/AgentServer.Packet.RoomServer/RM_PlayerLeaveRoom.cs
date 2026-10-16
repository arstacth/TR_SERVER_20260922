using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_PlayerLeaveRoom : NetPacket
	{
		public RM_PlayerLeaveRoom(Account User, bool isDisconnect, byte last)
		{
			ns.WriteOP(RMProtocol.RM_UserLeaveRoom_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(isDisconnect);
			ns.Write(last);
		}
	}
}
