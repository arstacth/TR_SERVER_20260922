using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AssaultModeUseItem_Ack : NetPacket
	{
		public AssaultModeUseItem_Ack(int itemnum, int itemcount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ASSAULT_MODE_USE_ITEM_ACK);
			ns.Write(itemnum);
			ns.Write(itemcount);
			_ = last;
		}
	}
}
