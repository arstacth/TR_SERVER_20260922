using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyJoinLimitLevelACK : NetPacket
	{
		public ModifyJoinLimitLevelACK(short joinLimitLevel, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MODIFY_JOIN_LIMIT_LEVEL_ACK);
			ns.Write(joinLimitLevel);
			_ = last;
		}
	}
}
