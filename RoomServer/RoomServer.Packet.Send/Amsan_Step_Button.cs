using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Amsan_Step_Button : NetPacket
	{
		public Amsan_Step_Button(int btnid, byte pos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_STEP_FOOT_BOARD_ACK);
			ns.Write(btnid);
			ns.Write(pos);
			_ = last;
		}
	}
}
