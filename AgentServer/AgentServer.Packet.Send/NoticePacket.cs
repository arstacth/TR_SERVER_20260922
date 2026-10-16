using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
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
