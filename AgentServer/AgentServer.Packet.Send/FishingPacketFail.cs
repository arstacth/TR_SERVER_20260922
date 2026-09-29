using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishingPacketFail : NetPacket
	{
		public FishingPacketFail(int err, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_PROC_FISHING_ACK);
			ns.Write(err);
		}
	}
}
