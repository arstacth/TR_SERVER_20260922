using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopUserBuyList_ACK : NetPacket
	{
		public GetShopUserBuyList_ACK(List<ShopBuyCountList> UserBuyCountList, List<GameDataShopPurchasingLimit> ShopPurchasingLimit, byte last)
		{
			_ = UserBuyCountList;
			_ = ShopPurchasingLimit;
			_ = last;
			ns.WriteOP(Opcodes.eServer_SHOP_USER_BUY_COUNT_LIST_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
		}
	}
}
