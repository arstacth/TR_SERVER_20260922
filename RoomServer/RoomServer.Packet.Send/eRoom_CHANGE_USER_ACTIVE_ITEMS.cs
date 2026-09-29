using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_CHANGE_USER_ACTIVE_ITEMS : NetPacket
	{
		public eRoom_CHANGE_USER_ACTIVE_ITEMS(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEMS);
			ns.Write(User.RoomPos);
			User.activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			_ = last;
		}
	}
}
