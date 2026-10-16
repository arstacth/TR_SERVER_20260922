using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DOLIMPAN_USE_CHARGE_COUPON_ACK : NetPacket
	{
		public DOLIMPAN_USE_CHARGE_COUPON_ACK(DolimpanHandle.Dolimpan result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DOLIMPAN_USE_CHARGE_COUPON_ACK);
			ns.Write((int)result);
			_ = last;
		}
	}
}
