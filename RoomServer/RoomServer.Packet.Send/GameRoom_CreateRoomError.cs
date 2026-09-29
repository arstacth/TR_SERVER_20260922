using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_CreateRoomError : NetPacket
	{
		public GameRoom_CreateRoomError(int errorid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MAKE_ROOM_FAILED_ACK);
			ns.Write(errorid);
			_ = last;
		}
	}
}
