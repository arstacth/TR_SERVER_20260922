using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildProcessJoinRequestACK : NetPacket
	{
		public GuildProcessJoinRequestACK(string name, byte accept, GuildMemberInfo memberinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.PROCESS_JOIN_REQUEST_ACK);
			ns.WriteAnsiFixed_intSize(name);
			ns.Write(accept);
			if (memberinfos != null)
			{
				ns.WriteAnsiFixed_intSize(memberinfos.nickname);
				ns.Write((short)0);
				ns.Write(memberinfos.FarmUniqueNum);
				ns.Write(memberinfos.ExpireDateTime);
				ns.Write((byte)1);
				ns.Write((short)0);
				ns.Write(memberinfos.lastLogoutTime);
				ns.Write((short)0);
				ns.Write(memberinfos.joinDate);
				ns.Write((int)memberinfos.Level);
				ns.Write((int)memberinfos.grade);
				ns.Write(memberinfos.contributionPoint);
				ns.Write((short)0);
				ns.Write(473162377);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
