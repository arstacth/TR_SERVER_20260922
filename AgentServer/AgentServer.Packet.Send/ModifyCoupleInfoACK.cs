using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyCoupleInfoACK : NetPacket
	{
		public ModifyCoupleInfoACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_MODIFY_COUPLE_INFO_ACK);
			_ = last;
		}
	}
}
