using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistMix_Fail : NetPacket
	{
		public AlchemistMix_Fail(eAlchemistMixFailedReason failReason, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_MIX_ACK);
			ns.Write(89);
			ns.Write((int)failReason);
			_ = last;
		}
	}
}
