using System.Collections.Generic;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopBuyLimitCountList : NetPacket
	{
		public GetShopBuyLimitCountList(byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_LIMIT_COUNT_LIST_ACK);
			ns.Write(0);
			List<ShopBuyLimitCount> list = ShopHolder.ShopBuyLimitCountList.Where((ShopBuyLimitCount c) => ShopHolder.PackedShopDisplayNums.Contains(c.ShopDisplayNum)).ToList();
			ns.Write(list.Count);
			foreach (ShopBuyLimitCount item in list)
			{
				ns.Write(item.ShopDisplayNum);
				ns.Write(item.DayPurchasingLimit);
				ns.Write(item.MonthPurchasingLimit);
				ns.Write(item.TotalPurchasingLimit);
				// 5th field: fdAccountPurchasingLimit — missing this caused endless EOF Overpop+4 (pink).
				ns.Write(item.AccountPurchasingLimit);
			}
		}
	}
}
