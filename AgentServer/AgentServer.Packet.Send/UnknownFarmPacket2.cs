using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownFarmPacket2 : NetPacket
	{
		public UnknownFarmPacket2(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			// Inner farm sub-opcode 127 (ACK).
			ns.Write((short)127);
			ns.Write(0);
			_ = last;
		}
	}
}
