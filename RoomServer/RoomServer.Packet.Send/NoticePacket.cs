using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class NoticePacket : NetPacket
	{
		public NoticePacket(string content, byte last)
		{
			ns.WriteOP(Opcodes.eServer_NOTICE_MSG_ACK);
			ns.Write(0);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(content);
			_ = last;
		}
	}
}
