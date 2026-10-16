using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SetClearLimitTime_Ack : NetPacket
	{
		public SetClearLimitTime_Ack(int sec, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_SET_CLEAR_LIMIT_TIME_ACK);
			ns.Write(sec);
			_ = last;
		}
	}
}
