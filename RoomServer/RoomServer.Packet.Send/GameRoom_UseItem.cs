using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_UseItem : NetPacket
	{
		public GameRoom_UseItem(byte pos, int time, int itemid, byte[] bytes, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_USE_GAMEROOM_ITEM_ACK);
			ns.Write(pos);
			ns.Write(time);
			ns.Write(itemid);
			ns.Write(bytes, 0);
			_ = last;
		}
	}
}
