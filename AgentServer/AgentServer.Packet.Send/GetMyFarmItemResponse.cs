using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMyFarmItemResponse : NetPacket
	{
		public GetMyFarmItemResponse(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetMyFarmItemList_ACK_1);
			_ = last;
		}
	}
}
