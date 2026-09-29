using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MapGenerateItem : NetPacket
	{
		public GameRoom_MapGenerateItem(int unk, short itemtype, int itemid, byte[] buffer, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MAKE_ITEM_FOR_MAP_GENERATE_ACK);
			ns.Write(unk);
			ns.Write(itemtype);
			ns.Write(itemid);
			ns.Write(buffer, 0);
			_ = last;
		}
	}
}
