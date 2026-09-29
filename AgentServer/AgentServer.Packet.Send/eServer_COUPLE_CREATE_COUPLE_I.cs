using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_COUPLE_CREATE_COUPLE_INFO_ACK : NetPacket
	{
		public eServer_COUPLE_CREATE_COUPLE_INFO_ACK(byte failedReason, int coupleNum, int coupleRingNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_CREATE_COUPLE_INFO_ACK);
			ns.Write(failedReason);
			ns.Write(coupleNum);
			ns.Write(coupleRingNum);
			_ = last;
		}
	}
}
