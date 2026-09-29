using System.Collections.Generic;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildJoinRequestListACK : NetPacket
	{
		public GuildJoinRequestListACK(List<GuildJoinRequestInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.GET_JOIN_REQUEST_LIST_ACK);
			ns.Write(infos.Count);
			foreach (GuildJoinRequestInfo info in infos)
			{
				ns.Write(info.exp);
				ns.Write(info.date);
				ns.WriteAnsiFixed_intSize(info.nickName);
				ns.WriteAnsiFixed_intSize(info.message);
				ns.Write(0);
				ns.Write((byte)0);
			}
			_ = last;
		}
	}
}
