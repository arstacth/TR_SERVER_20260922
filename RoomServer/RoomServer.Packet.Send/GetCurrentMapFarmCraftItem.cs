using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GetCurrentMapFarmCraftItem : NetPacket
	{
		public GetCurrentMapFarmCraftItem(List<FarmCraftMapItem> farmcraftmapitem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eServer_FARM_CRAFT_PROTOCOL);
			ns.Write(0);
			ns.Write(0);
			ns.Write(farmcraftmapitem.Count);
			foreach (FarmCraftMapItem item in farmcraftmapitem)
			{
				ns.Write(item.ItemKind);
				ns.Write(item.TotalCount);
				ns.Write(item.UsedCount);
			}
			_ = last;
		}
	}
}
