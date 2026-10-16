using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_EndLoading : NetPacket
	{
		public GameRoom_EndLoading(byte roompos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ALL_LOADING_END_ACK);
			ns.Write(roompos);
			_ = last;
		}
	}
}
