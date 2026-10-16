using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DOLIMPAN_RESET_ACK : NetPacket
	{
		public DOLIMPAN_RESET_ACK(DolimpanHandle.Dolimpan result, Tuple<int, int> ret, byte idx, int iCouponNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DOLIMPAN_RESET_ACK);
			ns.Write((int)result);
			if (result == DolimpanHandle.Dolimpan.eServerResult_OK_ACK)
			{
				ns.Write(ret.Item2);
				ns.Write(idx);
				ns.Write(iCouponNum);
			}
			_ = last;
		}
	}
}
