using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket14 : NetPacket
	{
		public UnknownPacket14(byte last)
		{
			ns.WriteOP(Opcodes.eServer_STANDBY_PURCHASE_DICISION_GET_LIST_ACK);
			ns.Write(0L);
			_ = last;
		}
	}
}
