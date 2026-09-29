using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_PickMapItem : NetPacket
	{
		public GameRoom_PickMapItem(byte roompos, int itemid, int itemtype, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_EAT_ITEM_FOR_MAP_GENERATE_ACK);
			ns.Write(roompos);
			ns.Write(1);
			ns.Write(itemid);
			ns.Write(itemtype);
			_ = last;
		}
	}
}
