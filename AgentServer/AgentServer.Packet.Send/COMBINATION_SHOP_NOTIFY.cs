using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class COMBINATION_SHOP_NOTIFY : NetPacket
	{
		public COMBINATION_SHOP_NOTIFY(byte combinationShop, bool bIsVisible)
		{
			ns.WriteOP(Opcodes.eServer_COMBINATION_SHOP_NOTIFY);
			ns.Write(combinationShop);
			ns.Write(bIsVisible);
			ns.Write((byte)1);
		}
	}
}
