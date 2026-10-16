using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyLeaveOK_Ack : NetPacket
	{
		public PartyLeaveOK_Ack(int type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(3L);
			ns.Write(type);
			_ = last;
		}
	}
}
