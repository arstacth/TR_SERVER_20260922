using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_LapTimeCountdwon2 : NetPacket
	{
		public GameRoom_LapTimeCountdwon2(NormalRoom room, short second, byte round, bool flag, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TIME_COUNT_END_ACK);
			ns.Write(round);
			ns.Write(second);
			ns.Write(flag);
			if (flag)
			{
				ns.Write(2);
				ns.Write(0);
				ns.Write((room.RuleType == 45568 || room.RuleType == 111104) ? 1 : 2);
			}
			_ = last;
		}
	}
}
