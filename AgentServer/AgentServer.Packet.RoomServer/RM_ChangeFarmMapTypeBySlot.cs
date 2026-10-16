using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_ChangeFarmMapTypeBySlot : NetPacket
	{
		public RM_ChangeFarmMapTypeBySlot(Account User, byte last)
		{
			ns.WriteOP(RMProtocol.RM_ChangeFarmMapTypeBySlot_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(last);
		}
	}
}
