using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomPosReady : NetPacket
	{
		public GameRoom_RoomPosReady(byte roompos, bool isReady, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOM_READY_ACK);
			ns.Write(roompos);
			ns.Write(isReady);
			_ = last;
		}
	}
}
