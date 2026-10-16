using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyRecruitCancelOK_Ack : NetPacket
	{
		public PartyRecruitCancelOK_Ack(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(15L);
			_ = last;
		}
	}
}
