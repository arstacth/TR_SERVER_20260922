using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DelGuildOKACK : NetPacket
	{
		public DelGuildOKACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.DEL_GUILD_ACK);
			_ = last;
		}
	}
}
