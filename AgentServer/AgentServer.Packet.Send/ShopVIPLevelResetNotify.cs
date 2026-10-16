using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ShopVIPLevelResetNotify : NetPacket
	{
		public ShopVIPLevelResetNotify()
		{
			ns.WriteOP(Opcodes.eServer_SHOP_USER_VIP_LEVEL_RESET_NOTIFY);
			ns.Write((byte)1);
		}
	}
}
