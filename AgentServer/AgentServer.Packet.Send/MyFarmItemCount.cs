using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyFarmItemCount : NetPacket
	{
		public MyFarmItemCount(int count, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetMyFarmItemList_ACK_3);
			ns.Write(0);
			ns.Write(count);
			_ = last;
		}
	}
}
