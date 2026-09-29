using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shop;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class COMBINATION_SHOP_GET_ITEM_DETAIL_INFO_ACK : NetPacket
	{
		public COMBINATION_SHOP_GET_ITEM_DETAIL_INFO_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMBINATION_SHOP_GET_ITEM_DETAIL_INFO_ACK);
			ns.Write(CombinationShopHolder.ExchangeItemInfo.Count);
			foreach (CombinationShopItemDetailInfo value in CombinationShopHolder.ExchangeItemInfo.Values)
			{
				ns.Write(value.m_iExchangeID);
				ns.Write(value.m_iExchangeItem);
				ns.Write(value.m_StartDate);
				ns.Write(value.m_EndDate);
				ns.Write(value.m_iLimitSellCount);
				ns.Write(value.m_iLimitUserBuyCount);
			}
			_ = last;
		}
	}
}
