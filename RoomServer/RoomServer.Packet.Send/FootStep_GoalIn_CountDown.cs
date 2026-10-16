using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FootStep_GoalIn_CountDown : NetPacket
	{
		public FootStep_GoalIn_CountDown(int laptime, int counttime, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_TIMEOUT_COUNT_ACK);
			ns.Write(laptime);
			ns.Write(counttime);
			_ = last;
		}
	}
}
