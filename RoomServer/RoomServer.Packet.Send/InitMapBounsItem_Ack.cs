using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class InitMapBounsItem_Ack : NetPacket
	{
		public InitMapBounsItem_Ack(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_BONUS_ITEM_MAKE_ACK);
			_ = last;
		}
	}
}
