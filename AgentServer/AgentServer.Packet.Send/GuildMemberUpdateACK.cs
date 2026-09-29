using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildMemberUpdateACK : NetPacket
	{
		public GuildMemberUpdateACK(short type, string NickName, int grade, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MEMBER_UPDATE_ACK);
			ns.Write(type);
			ns.WriteAnsiFixed_intSize(NickName);
			ns.Write(grade);
			ns.Write(0L);
			ns.Write(0L);
			_ = last;
		}
	}
}
