using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HuMongPickBoard_PickItem_OK : NetPacket
	{
		public HuMongPickBoard_PickItem_OK(short PickID, byte Rank, int AdditionRewardItemNum, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_HUMONG_PICKBOARD_PICK_ACK);
			ns.Write(0);
			ns.Write(PickID);
			ns.Write(Rank);
			ns.Write(-1);
			ns.Write(AdditionRewardItemNum);
		}
	}
}
