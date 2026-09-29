using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_GameRoomUpdateGuild : NetPacket
	{
		public RM_GameRoomUpdateGuild(Account User, string name, string guildname, int guildnum, int grade, int unk, short unk2, byte last)
		{
			ns.WriteOP(RMProtocol.RM_GameRoomUpdateGuild_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.WriteAnsiFixed_intSize(name);
			ns.WriteAnsiFixed_intSize(guildname);
			ns.Write(guildnum);
			ns.Write(grade);
			ns.Write(unk);
			ns.Write(unk2);
			ns.Write(last);
		}
	}
}
