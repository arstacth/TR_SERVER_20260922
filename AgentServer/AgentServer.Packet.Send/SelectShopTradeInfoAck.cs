using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SelectShopTradeInfoAck : NetPacket
	{
		public SelectShopTradeInfoAck(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SELECT_SHOP_TRADE_INFO_ACK);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}
}
