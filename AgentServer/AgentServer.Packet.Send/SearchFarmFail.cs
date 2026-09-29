using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SearchFarmFail : NetPacket
	{
		public SearchFarmFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SearchFarm_ACK);
			ns.Write(220);
			ns.Write(3);
			_ = last;
		}
	}
}
