using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_CHANGE_USER_ACTIVE_ITEMS : NetPacket
	{
		public eRoom_CHANGE_USER_ACTIVE_ITEMS(Account User, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEMS);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			User.activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			ns.Write(last);
		}
	}
}
