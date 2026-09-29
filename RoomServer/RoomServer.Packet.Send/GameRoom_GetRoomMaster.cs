using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GetRoomMaster : NetPacket
	{
		public GameRoom_GetRoomMaster(byte roompos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MASTER_PLAYER_INDEX_ACK);
			ns.Write(roompos);
			_ = last;
		}
	}
}
