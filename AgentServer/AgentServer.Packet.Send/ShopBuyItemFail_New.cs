using System.Collections.Generic;
using AgentServer.Packet;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopBuyItemFail_New : NetPacket
	{
		public ShopBuyItemFail_New(eShopFailed_REASON failedReason, List<ShopBuyItemInfo> buyitemOKlist, byte last)
		{
			_ = buyitemOKlist;
			int reason = (int)failedReason;
			if (reason == 0)
			{
				reason = (int)eShopFailed_REASON.eShopFailed_REASON_GAME_SERVER_PROBLEM;
			}
			// Thai client reads first int as fail reason and stops.
			// Extra zero padding left RemainSize=17 (PacketSize=23).
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_PRODUCTS_TOGETHER_ACK);
			ns.Write(reason);
			_ = last;
		}
	}
}
