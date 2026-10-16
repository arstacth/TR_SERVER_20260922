using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Amsan_LapTimeControl : NetPacket
	{
		public Amsan_LapTimeControl(int currenttime, int totaltime, int addtime, bool isCorrect, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_PASS_SURVIVAL_ARITHMETIC_CHECK_POINT_ACK);
			ns.Write(currenttime);
			ns.Write(totaltime);
			ns.Write(addtime);
			ns.Write(isCorrect);
			_ = last;
		}
	}
}
