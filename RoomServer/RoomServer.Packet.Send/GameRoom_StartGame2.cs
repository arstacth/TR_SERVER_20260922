using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartGame2 : NetPacket
	{
		public GameRoom_StartGame2(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_REQUEST_START_COUNTING_ACK);
			_ = last;
		}
	}
}
