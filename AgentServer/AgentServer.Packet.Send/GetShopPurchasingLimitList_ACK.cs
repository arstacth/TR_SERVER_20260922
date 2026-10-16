using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopPurchasingLimitList_ACK : NetPacket
	{
		public GetShopPurchasingLimitList_ACK(IEnumerable<GameDataShopPurchasingLimit> ShopPurchasingLimit, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_COUNT_LIST_ACK);
			ns.Write(0);
			ns.Write(ShopPurchasingLimit.Count());
			foreach (GameDataShopPurchasingLimit item in ShopPurchasingLimit)
			{
				ns.Write(item.ShopDisplayNum);
				ns.Write(item.TotalPurchasingLimit);
			}
			_ = last;
		}
	}
}
