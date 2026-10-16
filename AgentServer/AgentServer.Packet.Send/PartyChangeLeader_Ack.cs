using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyChangeLeader_Ack : NetPacket
	{
		public PartyChangeLeader_Ack(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(8L);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
