using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetGuildUserInfoACK : NetPacket
	{
		public GetGuildUserInfoACK(GuildUserInfo UserInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.GET_GUILD_USER_INFO_ACK);
			ns.Write(UserInfo.grade);
			ns.Write(UserInfo.contributionPoint);
			ns.Write(UserInfo.joinDate);
			ns.Write(UserInfo.guildKind);
			ns.Write(UserInfo.guildNum);
			_ = last;
		}
	}
}
