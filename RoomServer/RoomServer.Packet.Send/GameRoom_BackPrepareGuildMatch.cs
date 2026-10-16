using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_BackPrepareGuildMatch : NetPacket
	{
		public GameRoom_BackPrepareGuildMatch(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILDMATCH_CHANGE_ROOMKIND_NOTIFY);
			ns.Write(79);
			_ = last;
		}
	}
}
