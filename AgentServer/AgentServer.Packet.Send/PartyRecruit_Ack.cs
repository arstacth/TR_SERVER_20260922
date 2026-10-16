using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyRecruit_Ack : NetPacket
	{
		public PartyRecruit_Ack(int code, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(14);
			ns.Write(code);
			_ = last;
		}
	}
}
