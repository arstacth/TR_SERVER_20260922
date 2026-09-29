using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RemoveCoupleACK : NetPacket
	{
		public RemoveCoupleACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_REMOVE_COUPLE_INFO_ACK);
			_ = last;
		}
	}
}
