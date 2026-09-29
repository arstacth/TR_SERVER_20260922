using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_AcceptGiftFail : NetPacket
	{
		public Myroom_AcceptGiftFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_ACCEPT_GIFT_ACK);
			ns.Write(77);
			// Fail is result int only — trailing byte was RemainSize=1 on PacketSize=7.
			_ = last;
		}
	}
}
