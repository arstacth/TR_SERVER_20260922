using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomPosRelayTeam : NetPacket
	{
		public GameRoom_RoomPosRelayTeam(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SELECT_RELAY_TEAM_POSITION_OK_ACK);
			ns.Write(User.RoomPos);
			ns.Write(User.RelayTeamPos);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
