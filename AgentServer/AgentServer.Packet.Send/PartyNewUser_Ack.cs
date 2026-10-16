using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyNewUser_Ack : NetPacket
	{
		public PartyNewUser_Ack(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(6L);
			ns.WriteAnsiFixed_intSize(User.NickName);
			ns.Write(3);
			ns.Write(User.Level);
			ns.Write(0);
			_ = last;
		}
	}
}
