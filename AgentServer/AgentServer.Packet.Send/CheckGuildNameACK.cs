using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CheckGuildNameACK : NetPacket
	{
		public CheckGuildNameACK(byte type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.CHECK_GUILD_NAME_ACK);
			ns.Write(type);
			_ = last;
		}
	}
}
