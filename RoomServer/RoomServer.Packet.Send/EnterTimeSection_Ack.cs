using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class EnterTimeSection_Ack : NetPacket
	{
		public EnterTimeSection_Ack(byte area, int remaintime, int sec, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_ENTER_TIME_SECTION_ACK);
			ns.Write(area);
			ns.Write(remaintime);
			ns.Write(sec);
			_ = last;
		}
	}
}
