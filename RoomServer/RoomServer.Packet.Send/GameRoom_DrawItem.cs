using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_DrawItem : NetPacket
	{
		public GameRoom_DrawItem(byte pos, int time, int CapsuleID, int itemid, int NextCapsuleID, bool bMakeAndEat, int realitem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_EAT_ITEM_ACK);
			ns.Write(pos);
			ns.Write(CapsuleID);
			ns.Write(time);
			ns.Write(itemid);
			ns.Write(NextCapsuleID);
			ns.Write(bMakeAndEat);
			ns.Write(realitem);
			_ = last;
		}
	}
}
