using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_ChooseGuildForMatch : NetPacket
	{
		public RM_ChooseGuildForMatch(Account User, short status, int guildmatchroomid, byte last)
		{
			ns.WriteOP(RMProtocol.RM_ChooseGuildForMatch_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(status);
			ns.Write(guildmatchroomid);
			ns.Write(last);
		}
	}
}
