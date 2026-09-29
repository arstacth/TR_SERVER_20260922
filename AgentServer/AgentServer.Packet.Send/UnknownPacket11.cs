using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket11 : NetPacket
	{
		public UnknownPacket11(string value, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COLLECTION_MISSION_GET_USER_MISSION_ACK);
			ns.WriteHex(value);
			_ = last;
		}
	}
}
