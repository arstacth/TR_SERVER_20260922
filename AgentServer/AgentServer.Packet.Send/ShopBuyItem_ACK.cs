using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopBuyItem_ACK : NetPacket
	{
		public ShopBuyItem_ACK(List<ShopBuyItemInfo> buyitemOKlist, bool isFarmOrShuItem, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_PRODUCTS_TOGETHER_ACK);
			ns.Write(0);
			ns.Write(buyitemOKlist.Count);
			foreach (ShopBuyItemInfo item in buyitemOKlist)
			{
				ns.Write(item.ItemNum);
				ns.Write(item.unk3);
				// Client logs onRecvShopBuyProductsTogetherSuccess ID:… — hard -1 made Shu/farm buys show ID:-1.
				ns.Write(item.ItemID > 0 ? item.ItemID : -1L);
				ns.Write(0);
				ns.Write(item.unk4);
				ns.Write(item.ItemID > 0 ? item.ItemID : -1L);
				ns.Fill(20);
				ns.Write(item.SellItemNum);
			}
			if (isFarmOrShuItem)
			{
				ns.Write(buyitemOKlist.Count);
				foreach (ShopBuyItemInfo item2 in buyitemOKlist)
				{
					ns.Write(item2.ItemID);
					ns.Write(item2.supplyItemDescNum);
				}
			}
			else
			{
				ns.Write(0);
			}
			ns.Write((byte)0);
			ns.Write(buyitemOKlist.Count);
			foreach (ShopBuyItemInfo item3 in buyitemOKlist)
			{
				int granted = item3.supplyItemDescNum > 0 ? item3.supplyItemDescNum : item3.ItemNum;
				ns.Write(granted);
				ns.Write(0);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
