using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildProcessJoinRequestACK2 : NetPacket
	{
		public GuildProcessJoinRequestACK2(short type, string guildName, int guildNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MEMBER_UPDATE_ACK);
			ns.Write(type);
			ns.WriteAnsiFixed_intSize(guildName);
			ns.Write(guildNum);
			ns.Write(1L);
			ns.Write(0L);
			_ = last;
		}
	}
}
