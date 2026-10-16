using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Amsan_Step_Button_Push : NetPacket
	{
		public Amsan_Step_Button_Push(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_PASS_FOOT_BOARD_AREA_ACK);
			_ = last;
		}
	}
}
