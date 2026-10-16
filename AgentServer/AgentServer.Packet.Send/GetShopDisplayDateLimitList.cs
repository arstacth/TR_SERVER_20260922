using System.Collections.Generic;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopDisplayDateLimitList : NetPacket
	{
		public GetShopDisplayDateLimitList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_DISPLAY_DATE_LIST_ACK);
			ns.Write(0);
			List<ShopDisplayDateLimit> list = ShopHolder.ShopDisplayDateLimitList.Values.Where((ShopDisplayDateLimit c) => ShopHolder.PackedShopDisplayNums.Contains(c.ShopDisplayNum)).ToList();
			ns.Write(list.Count);
			foreach (ShopDisplayDateLimit item in list)
			{
				ns.Write(item.ShopDisplayNum);
				ns.Write(item.DisplayStartDate);
				ns.Write(item.DisplayEndDate);
				ns.Write(item.BuyStartDate);
				ns.Write(item.BuyEndDate);
			}
			_ = last;
		}
	}
}
