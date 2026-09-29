using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetCurrentMapFarmCraftItem_ShopBuy : NetPacket
	{
		public GetCurrentMapFarmCraftItem_ShopBuy(FarmCraftMapItem farmcraftmapitem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eServer_FARM_CRAFT_PROTOCOL);
			ns.Write(1);
			ns.Write(0);
			ns.Write(farmcraftmapitem.ItemKind);
			ns.Write(farmcraftmapitem.TotalCount);
			ns.Write(farmcraftmapitem.UsedCount);
			_ = last;
		}
	}
}
