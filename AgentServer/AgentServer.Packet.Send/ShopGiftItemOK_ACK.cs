using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopGiftItemOK_ACK : NetPacket
	{
		public ShopGiftItemOK_ACK(Account User, int itemid, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_GIFT_PRODUCTS_ACK);
			ns.Write(0);
			ns.Write(itemid);
			ns.Write(User.TR);
			ns.Write(500);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
