using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_AllSync : NetPacket
	{
		public GameRoom_AllSync(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ALL_LOADING_END_ALL_ROOM_USER_ACK);
			_ = last;
		}
	}
}
