using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TypingRun_ReqText : NetPacket
	{
		public TypingRun_ReqText(int index, int area, string text, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TYPEINGRUN_QUESTION_ACK);
			ns.Write(index);
			ns.Write(area);
			ns.WriteAnsiFixed_intSize(text);
			ns.Write(0);
			_ = last;
		}
	}
}
