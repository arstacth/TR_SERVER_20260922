using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_CHANGE_USER_AVATAR_LOCK : NetPacket
	{
		public eRoom_CHANGE_USER_AVATAR_LOCK(Account User, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_AVATAR_LOCK);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			User.avatarLock.encode(ns);
			ns.Write(last);
		}
	}
}
