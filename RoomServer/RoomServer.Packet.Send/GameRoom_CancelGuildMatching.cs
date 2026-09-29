using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_CancelGuildMatching : NetPacket
	{
		public GameRoom_CancelGuildMatching(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILDMATCH_UNREGIST_MATCH_ACK);
			ns.Write((byte)1);
			_ = last;
		}
	}
}
