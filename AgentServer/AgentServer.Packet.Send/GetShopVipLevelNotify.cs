using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopVipLevelNotify : NetPacket
	{
		public GetShopVipLevelNotify(int viplevel, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_USER_VIP_LEVEL_NOTIFY);
			ns.Write(viplevel);
			_ = last;
		}
	}
}
