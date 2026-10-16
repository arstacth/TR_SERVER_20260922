using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class IncreaseStrengthenCount_ACK : NetPacket
	{
		public IncreaseStrengthenCount_ACK(int itemNum, short strengthenCount, int increaseItemNum, int remainItemCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_RECHARGE_REINFORCE_CHANCE_OK_ACK);
			ns.Write(itemNum);
			ns.Write(strengthenCount);
			ns.Write(increaseItemNum);
			ns.Write(remainItemCount);
			_ = last;
		}
	}
}
