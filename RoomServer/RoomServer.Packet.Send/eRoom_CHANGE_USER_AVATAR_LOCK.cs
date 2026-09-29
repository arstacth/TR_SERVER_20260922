using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_CHANGE_USER_AVATAR_LOCK : NetPacket
	{
		public eRoom_CHANGE_USER_AVATAR_LOCK(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_AVATAR_LOCK);
			ns.Write(User.RoomPos);
			User.avatarLock.encode(ns);
			_ = last;
		}
	}
}
