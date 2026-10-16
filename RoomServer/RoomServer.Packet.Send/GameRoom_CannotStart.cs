using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_CannotStart : NetPacket
	{
		public GameRoom_CannotStart(byte errid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_LOADING_FAILED_ACK);
			ns.Write(errid);
			_ = last;
		}
	}
}
