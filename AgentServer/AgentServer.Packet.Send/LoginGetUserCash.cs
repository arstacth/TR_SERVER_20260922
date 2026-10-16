using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginGetUserCash : NetPacket
	{
		public LoginGetUserCash(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_REQUEST_CHECK_CASH_ACK);
			ns.Write(0);
			ns.Write(User.Cash);
			ns.Write(0);
			// 19:34 size 14 Remain OK with 3 ints and no last-byte. Cash 0
			// came from CASH_POINT_ACK stub, not this packet.
		}
	}
}
