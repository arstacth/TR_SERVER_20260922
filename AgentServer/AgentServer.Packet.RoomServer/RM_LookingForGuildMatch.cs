using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_LookingForGuildMatch : NetPacket
	{
		public RM_LookingForGuildMatch(Account User, short mode, byte last)
		{
			ns.WriteOP(RMProtocol.RM_LookingForGuildMatch_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(mode);
			ns.Write(last);
		}
	}
}
