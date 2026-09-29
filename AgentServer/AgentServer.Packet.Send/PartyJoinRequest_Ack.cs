using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyJoinRequest_Ack : NetPacket
	{
		public PartyJoinRequest_Ack(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(19L);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
