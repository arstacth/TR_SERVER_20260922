using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MiniGame_GetPoint : NetPacket
	{
		public GameRoom_MiniGame_GetPoint(Account User, int nowpoint, int getpoint, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MULTIMINIGAME_UPDATE_USER_POINT_ACK);
			ns.Write(User.RoomPos);
			ns.Write(nowpoint);
			ns.Write(getpoint);
			ns.Write(0);
			_ = last;
		}
	}
}
