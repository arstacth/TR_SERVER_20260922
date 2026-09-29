using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CashPointAck : NetPacket
	{
		public CashPointAck(Account user, byte last)
		{
			// Packed eServer_CASH_POINT_ACK wire 2382. 19:34 empty two-int
			// stub was Remain 1 size 11 and set UI cash to 0. Same size:
			// result 0 + Account.Cash (CHECK_CASH 802 already loaded DB).
			ns.Write((ushort)2382);
			ns.Write(0);
			ns.Write(user.Cash);
			_ = last;
		}
	}
}
