using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AcceptPartyInviteFail_Ack : NetPacket
	{
		public AcceptPartyInviteFail_Ack(int code, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(1);
			ns.Write(code);
			_ = last;
		}
	}
}
