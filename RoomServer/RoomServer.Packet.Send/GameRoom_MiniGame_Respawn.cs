using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MiniGame_Respawn : NetPacket
	{
		public GameRoom_MiniGame_Respawn(Account User, int pos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAMEUSER_REBIRTH_ACK);
			ns.Write(0L);
			ns.Write(pos);
			_ = last;
		}
	}
}
