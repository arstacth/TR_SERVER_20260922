using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_EnterRoomOK : NetPacket
	{
		public GameRoom_EnterRoomOK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_DATA_RECV_BEGIN_ACK);
			_ = last;
		}
	}
}
