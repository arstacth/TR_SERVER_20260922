using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AcquireEmblem_ACK : NetPacket
	{
		public AcquireEmblem_ACK(int emblemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EMBLEM_ACQUIRE_ACK);
			ns.Write(emblemNum);
			ns.Write(1);
			ns.Write(1);
			ns.Write(1);
			_ = last;
		}
	}
}
