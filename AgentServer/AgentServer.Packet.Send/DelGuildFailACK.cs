using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DelGuildFailACK : NetPacket
	{
		public DelGuildFailACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.DEL_GUILD_FAIL_ACK);
			ns.Write((short)12);
			_ = last;
		}
	}
}
