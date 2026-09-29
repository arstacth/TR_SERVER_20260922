using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ControlRoomPos : NetPacket
	{
		public GameRoom_ControlRoomPos(byte roompos, bool isOff, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_SLOT_STATE_ACK);
			ns.Write(roompos);
			ns.Write(isOff);
			_ = last;
		}
	}
}
