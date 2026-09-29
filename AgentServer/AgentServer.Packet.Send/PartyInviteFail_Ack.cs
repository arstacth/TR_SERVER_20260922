using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyInviteFail_Ack : NetPacket
	{
		public PartyInviteFail_Ack(int code, string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(0);
			ns.Write(code);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
