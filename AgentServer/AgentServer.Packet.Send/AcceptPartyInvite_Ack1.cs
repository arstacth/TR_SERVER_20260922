using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AcceptPartyInvite_Ack1 : NetPacket
	{
		public AcceptPartyInvite_Ack1(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(5L);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(name);
			ns.Write(0);
			_ = last;
		}
	}
}
