using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket3 : NetPacket
	{
		public UnknownPacket3(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_USER_WISH_LIST_ACK);
			ns.Write(0L);
			_ = last;
		}
	}
}
