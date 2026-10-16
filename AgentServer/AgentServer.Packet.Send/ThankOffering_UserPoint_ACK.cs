using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ThankOffering_UserPoint_ACK : NetPacket
	{
		public ThankOffering_UserPoint_ACK(int normalPoint, int hcPoint, byte last)
		{
			ns.WriteOP(Opcodes.eServer_THANK_OFFERING_PROTOCOL);
			ns.Write((int)eThankOfferingProtocol.GET_USER_POINT_REQ);
			ns.Write(0);
			ns.Write(normalPoint);
			ns.Write(hcPoint);
			_ = last;
		}
	}
}
