using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GiveUpItem : NetPacket
	{
		public GameRoom_GiveUpItem(Account User, bool realitem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_REMOVE_GAMEROOM_ITEM_ACK);
			ns.Write(User.RoomPos);
			ns.Write(realitem);
			_ = last;
		}
	}
}
