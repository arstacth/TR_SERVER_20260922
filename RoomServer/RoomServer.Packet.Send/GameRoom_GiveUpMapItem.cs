using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GiveUpMapItem : NetPacket
	{
		public GameRoom_GiveUpMapItem(Dictionary<int, short> RemoveMapItems, int time, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_DROP_ITEM_FOR_MAP_GENERATE_ACK);
			ns.Write(RemoveMapItems.Count);
			foreach (KeyValuePair<int, short> RemoveMapItem in RemoveMapItems)
			{
				ns.Write((int)RemoveMapItem.Value);
				ns.Write(RemoveMapItem.Key);
				ns.Write(time);
			}
			ns.Write(21217);
			_ = last;
		}
	}
}
