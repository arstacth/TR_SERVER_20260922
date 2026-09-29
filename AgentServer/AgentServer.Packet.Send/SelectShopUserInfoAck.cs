using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SelectShopUserInfoAck : NetPacket
	{
		public SelectShopUserInfoAck(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SELECT_SHOP_USER_INFO_ACK);
			ns.Write(0);
			ns.Write(User != null ? User.FreePassType : 0);
			ns.Write(0L);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}
}
