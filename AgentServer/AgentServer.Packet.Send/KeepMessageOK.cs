using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class KeepMessageOK : NetPacket
	{
		public KeepMessageOK(long messageNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_KEEP_ACK);
			ns.Write(0);
			ns.Write(messageNum);
			_ = last;
		}
	}
}
