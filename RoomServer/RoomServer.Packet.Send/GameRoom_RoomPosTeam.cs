using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomPosTeam : NetPacket
	{
		public GameRoom_RoomPosTeam(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SELECT_TEAM_OK_ACK);
			ns.Write(User.RoomPos);
			ns.Write(User.Team);
			_ = last;
		}
	}
}
