using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CollectFishedItemFail : NetPacket
	{
		public CollectFishedItemFail(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_RECEIVE_FROM_KEEP_NET_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
