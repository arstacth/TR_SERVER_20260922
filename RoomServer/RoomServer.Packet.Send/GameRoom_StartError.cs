using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartError : NetPacket
	{
		public GameRoom_StartError(int error, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_REUQEST_CANT_START_COUNTING_ACK);
			ns.Write(error);
			_ = last;
		}
	}
}
