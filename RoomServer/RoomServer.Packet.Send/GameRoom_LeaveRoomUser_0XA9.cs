using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_LeaveRoomUser_0XA9 : NetPacket
	{
		public GameRoom_LeaveRoomUser_0XA9(byte roompos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LEAVE_ROOM_TO_ME_ACK);
			ns.Write(roompos);
			_ = last;
		}
	}
}
