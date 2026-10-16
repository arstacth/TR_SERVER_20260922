using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ReadMessageOK : NetPacket
	{
		public ReadMessageOK(long messageNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_READ_ACK);
			ns.Write(0);
			ns.Write(messageNum);
			_ = last;
		}
	}
}
