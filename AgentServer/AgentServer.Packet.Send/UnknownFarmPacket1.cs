using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownFarmPacket1 : NetPacket
	{
		public UnknownFarmPacket1(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.FarmProtocol_UNK1_ACK);
			_ = last;
		}
	}
}
