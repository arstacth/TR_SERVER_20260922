using System.Collections.Generic;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CM_GetGuildMemberACK : NetPacket
	{
		public CM_GetGuildMemberACK(byte type, List<GuildMemberInfo> memberinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_GUILD_MEMBER_LIST_ACK);
			ns.Write(type);
			ns.Write(memberinfos.Count);
			foreach (GuildMemberInfo memberinfo in memberinfos)
			{
				ns.WriteAnsiFixed_intSize(memberinfo.nickname);
				ns.Write((short)0);
				ns.Write(memberinfo.FarmUniqueNum);
				ns.Write(memberinfo.ExpireDateTime);
				ns.Write((byte)1);
				ns.Write((short)0);
				ns.Write(memberinfo.lastLogoutTime);
				ns.WriteAnsiFixed_intSize(memberinfo.memo);
				ns.Write(memberinfo.joinDate);
				ns.Write(memberinfo.Level);
				ns.Write((int)memberinfo.grade);
				ns.Write(memberinfo.contributionPoint);
				ns.Write(0);
				ns.Write(0L);
				ns.Write(0L);
			}
		}
	}
}
