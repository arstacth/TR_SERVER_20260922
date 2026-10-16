using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class MoveToGameRoom : NetPacket
	{
		public MoveToGameRoom(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MOVE_TO_WAIT_ROOM);
			_ = last;
		}
	}
}
