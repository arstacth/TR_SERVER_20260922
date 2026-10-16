using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyJoinRequestFail_Ack : NetPacket
	{
		public PartyJoinRequestFail_Ack(int errcode, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(20);
			ns.Write(errcode);
			_ = last;
		}
	}
}
