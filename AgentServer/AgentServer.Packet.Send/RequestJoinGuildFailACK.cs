using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RequestJoinGuildFailACK : NetPacket
	{
		public RequestJoinGuildFailACK(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.REQUEST_JOIN_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
