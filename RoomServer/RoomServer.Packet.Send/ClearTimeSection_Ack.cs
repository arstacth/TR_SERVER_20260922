using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ClearTimeSection_Ack : NetPacket
	{
		public ClearTimeSection_Ack(byte area, int ClearTime, int sec, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_CLEAR_TIME_SECTION_ACK);
			ns.Write(area);
			ns.Write(ClearTime);
			ns.Write(sec);
			_ = last;
		}
	}
}
