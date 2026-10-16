using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket13 : NetPacket
	{
		public UnknownPacket13(byte last)
		{
			ns.WriteOP(Opcodes.eServer_EMBLEM_USER_INFO_LIST_ACK);
			ns.WriteHex("010000002900000001000000E03700000100009F");
			_ = last;
		}
	}
}
