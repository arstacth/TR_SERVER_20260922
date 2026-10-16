using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopBuyAddSellPriceList : NetPacket
	{
		public GetShopBuyAddSellPriceList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_BUY_ADD_SELL_PRICE_LIST_ACK);
			ns.Write(0L);
			_ = last;
		}
	}
}
