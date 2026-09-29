using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_Alive : NetPacket
	{
		public GameRoom_Alive(byte pos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAME_OVER_SURVIVAL_ACK);
			ns.Write(1);
			ns.Write(pos);
			_ = last;
		}
	}
}
