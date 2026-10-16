using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartTimeOutCount : NetPacket
	{
		public GameRoom_StartTimeOutCount(int LapTime, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_TIME_BOMB_ACK);
			ns.Write(LapTime);
			ns.Write((byte)10);
			_ = last;
		}
	}
}
