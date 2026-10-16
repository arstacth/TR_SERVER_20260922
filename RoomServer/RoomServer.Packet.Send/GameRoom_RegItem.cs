using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RegItem : NetPacket
	{
		public GameRoom_RegItem(int time, int itemid, int CapsuleNum, byte[] bytes, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MAKE_ITEM_ACK);
			ns.Write(time);
			ns.Write(itemid);
			ns.Write(CapsuleNum);
			ns.Write(bytes, 0);
			_ = last;
		}
	}
}
