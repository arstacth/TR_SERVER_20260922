using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SearchFarmByUserNumOK : NetPacket
	{
		public SearchFarmByUserNumOK(int FarmUniqueNum, bool isNoPW, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SearchFarmByUserNum_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(isNoPW);
			_ = last;
		}
	}
}
