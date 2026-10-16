using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FarmPoint : NetPacket
	{
		public FarmPoint(int FarmPoint, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetFarmPoint_ACK);
			ns.Write(0);
			ns.Write(FarmPoint);
			_ = last;
		}
	}
}
