using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyInvite_Ack : NetPacket
	{
		public PartyInvite_Ack(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(0L);
			ns.Write(7);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
