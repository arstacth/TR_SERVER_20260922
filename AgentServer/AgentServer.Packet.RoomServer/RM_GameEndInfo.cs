using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_GameEndInfo : NetPacket
	{
		public RM_GameEndInfo(Account User, UserGameEndInfo info, byte last)
		{
			ns.WriteOP(RMProtocol.RM_UserGameEndInfo_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(info.racedistance);
			ns.Write(info.MapMaxDistance);
			ns.Write(info.gameendtype);
			ns.Write(last);
		}
	}
}
