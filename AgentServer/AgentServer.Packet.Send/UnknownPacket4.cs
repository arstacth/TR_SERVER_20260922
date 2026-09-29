using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket4 : NetPacket
	{
		public UnknownPacket4(byte last)
		{
			ns.WriteOP(Opcodes.eServer_IS_EXIST_CONFIRMATION_PASSWORD_ACK);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
