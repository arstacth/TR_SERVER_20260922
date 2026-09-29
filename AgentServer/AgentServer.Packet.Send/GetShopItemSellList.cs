using System.Collections.Generic;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 665 from debug_trgame 0x75BE30:
	/// result i32 + count i32 + rows:
	/// i32 i32 i32 u8 i32 u8 i32 u8 i32 u8 + vec i32 count/elems + u8 + i32.
	/// </summary>
	public sealed class GetShopItemSellList : NetPacket
	{
		public GetShopItemSellList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_ITEL_SELL_LIST_ACK);
			ns.Write(0);
			List<ShopItemSell> rows = ShopHolder.ShopItemSellList.Values
				.Where((ShopItemSell s) => ShopHolder.PackedShopSellItemNums.Contains(s.SellItemNum) && s.ShopDisplay)
				.OrderBy((ShopItemSell s) => s.SellItemNum)
				.ToList();
			if (rows.Count == 0)
			{
				rows = ShopHolder.ShopItemSellList.Values
					.Where((ShopItemSell s) => s.ShopDisplay)
					.OrderBy((ShopItemSell s) => s.SellItemNum)
					.Take(5)
					.ToList();
			}
			if (rows.Count > 5)
			{
				rows = rows.Take(5).ToList();
			}
			ns.Write(rows.Count);
			foreach (ShopItemSell s in rows)
			{
				ns.Write(s.SellItemNum);
				ns.Write(s.ShopDisplayNum);
				ns.Write(s.ItemNum);
				ns.Write(s.OriginCostType);
				ns.Write(s.OriginPrice);
				ns.Write(s.CostType);
				ns.Write(s.Price);
				ns.Write(s.MileageType);
				ns.Write(s.Mileage);
				ns.Write(s.ShopDisplay);
				// empty int vector
				ns.Write(0);
				ns.Write((byte)0);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
