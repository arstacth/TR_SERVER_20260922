using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopClosed : NetPacket
	{
		public ShopClosed(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_CANNOT_BUY_ITEM_BY_SERVER_SETTING_ACK);
			_ = last;
		}
	}
}
