using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket12 : NetPacket
	{
		public UnknownPacket12(byte last)
		{
			ns.WriteOP(Opcodes.eServer_EMBLEM_EVENT_LIST_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
