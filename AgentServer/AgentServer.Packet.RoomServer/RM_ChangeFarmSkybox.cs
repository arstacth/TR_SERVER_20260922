using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_ChangeFarmSkybox : NetPacket
	{
		public RM_ChangeFarmSkybox(Account User, int ItemNum, byte last)
		{
			ns.WriteOP(RMProtocol.RM_ChangeFarmSkybox_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(ItemNum);
			ns.Write(last);
		}
	}
}
