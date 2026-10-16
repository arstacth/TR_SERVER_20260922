using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TypingRun_EnterText : NetPacket
	{
		public TypingRun_EnterText(int index, int rank, int count, int time, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TYPEINGRUN_TYPING_DONE_ACK);
			ns.Write(index);
			ns.Write(rank);
			ns.Write(count);
			ns.Write(time);
			_ = last;
		}
	}
}
