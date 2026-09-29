using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SendMessageFail : NetPacket
	{
		public SendMessageFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_SEND_ACK);
			ns.Write(73);
			ns.Write((byte)7);
			_ = last;
		}
	}
}
