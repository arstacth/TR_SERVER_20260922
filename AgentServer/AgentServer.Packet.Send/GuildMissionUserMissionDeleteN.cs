using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildMissionUserMissionDeleteNotify_ACK : NetPacket
	{
		public GuildMissionUserMissionDeleteNotify_ACK(int type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_MISSION_USER_MISSION_DELETE_NOTIFY);
			ns.Write(type);
			_ = last;
		}
	}
}
