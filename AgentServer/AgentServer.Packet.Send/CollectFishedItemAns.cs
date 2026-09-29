using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CollectFishedItemAns : NetPacket
	{
		public CollectFishedItemAns(byte ans1, byte ans2, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_RECEIVE_FROM_KEEP_NET_ACK);
			ns.Write(0);
			ns.Write(ans1);
			ns.Write(ans2);
			_ = last;
		}
	}
}
