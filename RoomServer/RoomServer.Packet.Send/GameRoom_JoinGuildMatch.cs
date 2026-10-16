using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
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
