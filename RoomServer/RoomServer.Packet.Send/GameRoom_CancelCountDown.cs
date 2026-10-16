using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_CancelCountDown : NetPacket
	{
		public GameRoom_CancelCountDown(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_REQUEST_START_COUNTING_CANCEL_ACK);
			_ = last;
		}
	}
}
