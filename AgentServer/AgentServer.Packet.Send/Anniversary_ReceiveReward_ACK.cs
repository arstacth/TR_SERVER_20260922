using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_ReceiveReward_ACK : NetPacket
	{
		public Anniversary_ReceiveReward_ACK(int iObjectNum, byte grade, int rewardItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANNIVERSARY_GET_REWARD_GRADE_LIST_ACK);
			ns.Write((byte)0);
			ns.Write(iObjectNum);
			ns.Write(grade);
			ns.Write(rewardItemNum);
			_ = last;
		}
	}
}
