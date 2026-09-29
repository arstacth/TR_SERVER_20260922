using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildGetContributionPointACK : NetPacket
	{
		public GuildGetContributionPointACK(int point, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.GET_CONTRIBUTION_POINT_ACK);
			ns.Write(point);
			_ = last;
		}
	}
}
