using System.Collections.Generic;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopBuyAddBenefitList : NetPacket
	{
		public GetShopBuyAddBenefitList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_ADD_BENEFIT_LIST_ACK);
			ns.Write(0);
			List<ShopBuyAddBenefit> list = ShopHolder.ShopBuyAddBenefitList.Where((ShopBuyAddBenefit c) => ShopHolder.PackedShopSellItemNums.Contains(c.SellItemNum)).ToList();
			ns.Write(list.Count);
			foreach (ShopBuyAddBenefit item in list)
			{
				ns.Write(item.SellItemNum);
				ns.Write(item.ItemNum);
				ns.Write(item.Count);
			}
			_ = last;
		}
	}
}
