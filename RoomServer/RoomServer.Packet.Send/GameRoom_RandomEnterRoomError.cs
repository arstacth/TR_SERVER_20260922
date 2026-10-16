using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RandomEnterRoomError : NetPacket
	{
		public GameRoom_RandomEnterRoomError(byte roomkindid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_ACK);
			ns.Write(67);
			_ = roomkindid;
			_ = last;
		}
	}
}
