using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HuMongPickBoard_GiveItem_OK : NetPacket
	{
		public HuMongPickBoard_GiveItem_OK(short PickID, int ItemNum, int AdditionRewardItemNum, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_HUMONG_PICKBOARD_CONFIRM_ACK);
			ns.Write(0);
			ns.Write(PickID);
			ns.Write(ItemNum);
			ns.Write(AdditionRewardItemNum);
		}
	}
}
