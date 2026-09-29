using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_CancelLookingForGuildMatch : NetPacket
	{
		public RM_CancelLookingForGuildMatch(Account User, byte last)
		{
			ns.WriteOP(RMProtocol.RM_CancelLookingForGuildMatch_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(last);
		}
	}
}
