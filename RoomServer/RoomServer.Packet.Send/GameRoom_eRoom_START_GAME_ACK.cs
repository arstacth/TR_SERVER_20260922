using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_eRoom_START_GAME_ACK : NetPacket
	{
		public GameRoom_eRoom_START_GAME_ACK(Account User, int iGameStartTick, int iNumberOfItem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_GAME_ACK);
			ns.Write(iGameStartTick);
			ns.Write(iNumberOfItem);
			ns.Write(530084);
			_ = last;
		}
	}
}
