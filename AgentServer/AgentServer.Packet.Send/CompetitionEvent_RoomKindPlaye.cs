using System.Collections.Generic;
using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Room;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_RoomKindPlayerNum : NetPacket
	{
		public CompetitionEvent_RoomKindPlayerNum(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_PLAYERCOUNT_LIST_ACK);
			// Send every enabled competition room kind. Filtering by
			// competitionEventUsingRoomKind dropped the list from 35→17 and left
			// Animal Village / event tabs with no enterable modes.
			List<KeyValuePair<int, RoomKind_UserMinMax>> rows = new List<KeyValuePair<int, RoomKind_UserMinMax>>();
			foreach (KeyValuePair<int, RoomKind_UserMinMax> item in RoomHolder.RoomKindPlayerNum)
			{
				rows.Add(item);
			}
			ns.Write(rows.Count);
			foreach (KeyValuePair<int, RoomKind_UserMinMax> item in rows)
			{
				ns.Write(item.Key);
				ns.Write(item.Value.MinUser);
				ns.Write(item.Value.MaxUser);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
