using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildProcessJoinRequestFailACK : NetPacket
	{
		public GuildProcessJoinRequestFailACK(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.PROCESS_JOIN_REQUEST_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
