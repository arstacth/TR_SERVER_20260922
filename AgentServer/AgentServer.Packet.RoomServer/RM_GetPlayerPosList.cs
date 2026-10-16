using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_GetPlayerPosList : NetPacket
	{
		public RM_GetPlayerPosList(Account User, byte last)
		{
			ns.WriteOP(RMProtocol.RM_GetPlayerPosList_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(last);
		}
	}
}
