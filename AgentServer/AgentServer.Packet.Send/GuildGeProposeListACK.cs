using System.Collections.Generic;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildGeProposeListACK : NetPacket
	{
		public GuildGeProposeListACK(List<GuildInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.GET_PROPOSE_LIST_ACK);
			ns.Write(infos.Count);
			foreach (GuildInfo info in infos)
			{
				ns.Write((long)info.guildNum);
				ns.WriteAnsiFixed_intSize(info.guildName);
				ns.WriteAnsiFixed_intSize(info.masterName);
				ns.Write(info.foundationDate);
				ns.Write(info.memberCount);
				ns.Write(info.memberLimit);
				ns.Write(info.joinLimitLevelOver);
				ns.Write(info.joinLimitLevelBelow);
				ns.Write(info.joinMethod);
				ns.Write((short)info.level);
				ns.WriteAnsiFixed_intSize(info.message);
			}
			_ = last;
		}
	}
}
