using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyLeaveUser_Ack : NetPacket
	{
		public PartyLeaveUser_Ack(string name, int type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(7L);
			ns.WriteAnsiFixed_intSize(name);
			ns.Write(type);
			_ = last;
		}
	}
}
