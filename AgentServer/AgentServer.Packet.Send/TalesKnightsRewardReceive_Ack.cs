using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnightsRewardReceive_Ack : NetPacket
	{
		public TalesKnightsRewardReceive_Ack(int ItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_PVE_REWARD_RECEIVE_ACK);
			ns.Write(1);
			ns.Write(ItemNum);
			_ = last;
		}
	}
}
