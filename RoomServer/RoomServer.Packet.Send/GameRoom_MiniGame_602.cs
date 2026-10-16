using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MiniGame_602 : NetPacket
	{
		public GameRoom_MiniGame_602(Account User, int round, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAMEROUND_INFO_ACK);
			ns.Write(round);
			ns.Write(0L);
			_ = last;
		}
	}
}
