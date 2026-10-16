using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_KickPlayer : NetPacket
	{
		public RM_KickPlayer(Account User, string kickedplayername, byte last)
		{
			ns.WriteOP(RMProtocol.RM_KickPlayer_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.WriteAnsiFixed_intSize(kickedplayername);
			ns.Write(last);
		}
	}
}
