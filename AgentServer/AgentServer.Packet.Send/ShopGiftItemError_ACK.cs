using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopGiftItemError_ACK : NetPacket
	{
		public ShopGiftItemError_ACK(eGiftItemFailed failedReason, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_GIFT_PRODUCTS_ACK);
			ns.Write(75);
			_ = failedReason;
			_ = last;
		}
	}
}
