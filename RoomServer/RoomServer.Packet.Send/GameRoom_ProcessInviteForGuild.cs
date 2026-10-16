using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ProcessInviteForGuildMatch : NetPacket
	{
		public GameRoom_ProcessInviteForGuildMatch(short status, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILDMATCH_MATCH_ACTION_ACK);
			ns.Write(status);
			_ = last;
		}
	}
}
