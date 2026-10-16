using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopCategoryDisplayItem : NetPacket
	{
		public GetShopCategoryDisplayItem(int cat1, int cat2, int cat3, IEnumerable<ShopCategoryPurchasing> list, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_RANK_ACK);
			ns.Write(0);
			ns.Write(cat1);
			ns.Write(cat2);
			ns.Write(cat3);
			ns.Write(list.Count());
			foreach (ShopCategoryPurchasing item in list)
			{
				ns.Write(item.Order);
				ns.Write(item.ShopDisplayNum);
			}
			_ = last;
		}
	}
}
