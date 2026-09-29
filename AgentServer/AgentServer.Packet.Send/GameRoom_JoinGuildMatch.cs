using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameRoom_JoinGuildMatch : NetPacket
	{
		public GameRoom_JoinGuildMatch(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_GET_PARTY_ID_ACK);
			ns.Write(Rooms.GuildMatchRoomID);
			ns.Write(1);
			_ = last;
		}
	}
}
