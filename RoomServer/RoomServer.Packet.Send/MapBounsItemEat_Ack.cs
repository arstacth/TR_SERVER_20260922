using LocalCommons.Network;
using RoomServer.Structuring.Map;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class MapBounsItemEat_Ack : NetPacket
	{
		public MapBounsItemEat_Ack(int id, BonusItemInfo info, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_BONUS_ITEM_EAT_ACK);
			ns.Write(id);
			ns.Write(info.BonusType);
			ns.Write(0);
			ns.Write(info.BonusValue);
			ns.Write(int.MaxValue);
			_ = last;
		}
	}
}
