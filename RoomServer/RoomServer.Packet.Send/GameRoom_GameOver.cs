using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GameOver : NetPacket
	{
		public GameRoom_GameOver(byte pos, int type, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAME_OVER_ACK);
			ns.Write(pos);
			ns.Write(type);
			_ = last;
		}
	}
}
