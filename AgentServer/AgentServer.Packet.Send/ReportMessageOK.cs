using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ReportMessageOK : NetPacket
	{
		public ReportMessageOK(long messageNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_ACCUSE_ACK);
			ns.Write(0);
			ns.Write(messageNum);
			_ = last;
		}
	}
}
