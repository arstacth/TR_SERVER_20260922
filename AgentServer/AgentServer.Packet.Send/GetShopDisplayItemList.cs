using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopDisplayItemList : NetPacket
	{
		public GetShopDisplayItemList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_DISPLAY_LIST_ACK);
			ns.Write(0);
			ns.Write(ShopHolder.PackedShopDisplayItems.Count);
			foreach (ShopDisplayItem item in ShopHolder.PackedShopDisplayItems)
			{
				ns.Write(item.ShopDisplayNum);
				ns.Write(item.Category1);
				ns.Write(item.Category2);
				ns.Write(item.Category3);
				ns.Write(item.DisplaySortNum);
				ns.Write(item.ItemNum);
				ns.Write(item.Gift);
				ns.WriteAnsiFixed_intSize(item.ItemTag);
				ns.WriteAnsiFixed_intSize(item.Desc);
			}
			_ = last;
		}
	}
}
