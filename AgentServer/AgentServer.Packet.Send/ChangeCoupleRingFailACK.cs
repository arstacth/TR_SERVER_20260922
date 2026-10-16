using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ChangeCoupleRingFailACK : NetPacket
	{
		public ChangeCoupleRingFailACK(byte failedReason, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_CHANGE_COUPLE_RING_FAILED_ACK);
			ns.Write(failedReason);
			_ = last;
		}
	}
}
