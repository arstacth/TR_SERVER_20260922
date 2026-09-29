using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartGuildMatching : NetPacket
	{
		public GameRoom_StartGuildMatching(short mode, bool canStart, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILDMATCH_REGIST_MATCH_ACK);
			ns.Write(mode);
			ns.Write(canStart);
			_ = last;
		}
	}
}
