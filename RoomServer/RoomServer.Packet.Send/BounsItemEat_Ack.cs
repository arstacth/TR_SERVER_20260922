using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class BounsItemEat_Ack : NetPacket
	{
		public BounsItemEat_Ack(int type, int unk2, int unk3, int totalpoint, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_EAT_ITEM_ACK);
			ns.Write(type);
			ns.Write(unk2);
			ns.Write(unk3);
			ns.Write(totalpoint);
			_ = last;
		}
	}
}
