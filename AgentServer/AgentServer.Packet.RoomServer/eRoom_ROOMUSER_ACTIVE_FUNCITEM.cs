using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT : NetPacket
	{
		public eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT(Account User, List<int> items, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(items.Count);
			foreach (int item in items)
			{
				ns.Write(item);
			}
			ns.Write(last);
		}
	}
}
