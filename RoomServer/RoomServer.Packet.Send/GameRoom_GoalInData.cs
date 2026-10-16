using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GoalInData : NetPacket
	{
		public GameRoom_GoalInData(byte pos, int LapTime, int flag, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GOAL_IN_ACK);
			ns.Write(pos);
			ns.Write(LapTime);
			ns.Write(flag);
			_ = last;
		}
	}
}
