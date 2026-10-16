using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SearchFarmOK : NetPacket
	{
		public SearchFarmOK(int FarmUniqueNum, bool isNoPW, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SearchFarm_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(isNoPW);
			_ = last;
		}
	}
}
