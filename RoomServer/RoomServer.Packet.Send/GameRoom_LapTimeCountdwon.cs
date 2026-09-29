using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_LapTimeCountdwon : NetPacket
	{
		public GameRoom_LapTimeCountdwon(short second, byte round, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TIME_COUNT_START_ACK);
			ns.Write((byte)1);
			ns.Write(second);
			ns.Write(round);
			_ = last;
		}
	}
}
