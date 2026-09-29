using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MakeGuildFailACK : NetPacket
	{
		public MakeGuildFailACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MAKE_GUILD_FAIL_ACK);
			ns.Write((short)17);
			_ = last;
		}
	}
}
