using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_CHANGE_USER_ITEM_ATTR : NetPacket
	{
		public eRoom_CHANGE_USER_ITEM_ATTR(Account User, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ITEM_ATTR);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			User.mixedItemAttr.encodeUserItemAttr(ns);
			ns.Write(last);
		}
	}
}
