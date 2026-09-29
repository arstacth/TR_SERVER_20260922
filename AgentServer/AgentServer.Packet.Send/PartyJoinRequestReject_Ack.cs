using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyJoinRequestReject_Ack : NetPacket
	{
		public PartyJoinRequestReject_Ack(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(24L);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
