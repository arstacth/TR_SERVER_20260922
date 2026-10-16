using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ThankOffering_OnOff_ACK : NetPacket
	{
		public ThankOffering_OnOff_ACK(bool isopen, byte last)
		{
			ns.WriteOP(Opcodes.eServer_THANK_OFFERING_PROTOCOL);
			ns.Write((int)eThankOfferingProtocol.ON_OFF_ACK);
			ns.Write(0);
			ns.Write(isopen);
			_ = last;
		}
	}
}
