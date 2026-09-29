using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_PickGuildForMatch : NetPacket
	{
		public RM_PickGuildForMatch(Account User, short mode, byte last)
		{
			ns.WriteOP(RMProtocol.RM_PickGuildForMatch_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(mode);
			ns.Write(last);
		}
	}
}
