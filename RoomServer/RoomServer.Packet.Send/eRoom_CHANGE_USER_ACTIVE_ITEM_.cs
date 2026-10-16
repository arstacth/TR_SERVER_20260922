using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using TRCommon;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_CHANGE_USER_ACTIVE_ITEM_ONE : NetPacket
	{
		public eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(Account User, byte byChangeFlag, CActiveItems activeItem, CUserItemAttrManager useritemattr, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEM_ONE);
			ns.Write(User.RoomPos);
			ns.Write(byChangeFlag);
			activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			useritemattr.encodeUserItemAttr(ns);
			_ = last;
		}
	}
}
