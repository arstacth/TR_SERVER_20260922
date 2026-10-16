using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FarmExchangeItemFail_Ack : NetPacket
	{
		public FarmExchangeItemFail_Ack(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_HARVEST_ITEM_EXCHANGE_REWARD_FAILED_ACK);
			_ = last;
		}
	}
}
