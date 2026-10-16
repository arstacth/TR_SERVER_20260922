using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class COMBINATION_SHOP_GET_USE_INFO_ACK : NetPacket
	{
		public COMBINATION_SHOP_GET_USE_INFO_ACK(int systemType, eCombinationShopResult result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMBINATION_SHOP_GET_USE_INFO_ACK);
			ns.Write(systemType);
			ns.Write((int)result);
			_ = last;
		}
	}
}
