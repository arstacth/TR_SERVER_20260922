using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FarmExchangeItem_Ack : NetPacket
	{
		public FarmExchangeItem_Ack(int itemnum, int count, List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_HARVEST_ITEM_EXCHANGE_REWARD_ACK);
			ns.Write(itemnum);
			ns.Write(count);
			ns.Write(exinfo.Count);
			foreach (ExchangeItemInfo item in exinfo)
			{
				ns.Write(item.type);
				ns.Write(item.id);
				ns.Write(item.count);
				ns.Write(int.MaxValue);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
