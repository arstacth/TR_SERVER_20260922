using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_CHANGE_USER_ITEM_ATTR : NetPacket
	{
		public eRoom_CHANGE_USER_ITEM_ATTR(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ITEM_ATTR);
			ns.Write(User.RoomPos);
			User.userItemAttr.encodeUserItemAttr(ns);
			User.userItemAttr.encodeUserCharAttr(ns);
			User.charAbilityAttrMakeAttr();
			_ = last;
		}
	}
}
