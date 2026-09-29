using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket1 : NetPacket
	{
		public UnknownPacket1(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_GET_FRIEND_ADD_COUNT_ACK);
			ns.Write(0L);
			_ = last;
		}
	}
}
