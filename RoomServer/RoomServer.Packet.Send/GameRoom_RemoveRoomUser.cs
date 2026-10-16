using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RemoveRoomUser : NetPacket
	{
		public GameRoom_RemoveRoomUser(byte roompos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LEAVE_ROOM_ACK);
			ns.Write(roompos);
			_ = last;
		}
	}
}
