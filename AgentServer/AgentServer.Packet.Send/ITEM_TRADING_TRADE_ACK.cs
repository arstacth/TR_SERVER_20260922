using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ITEM_TRADING_TRADE_ACK : NetPacket
	{
		public ITEM_TRADING_TRADE_ACK(eTRADE_RESULT result, int source_item, int source_ticket, List<TRADE_RESULT> trs, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_TRADING_TRADE);
			ns.Write((byte)result);
			ns.Write(source_item);
			ns.Write(source_ticket);
			ns.Write(trs.Count);
			foreach (TRADE_RESULT tr in trs)
			{
				ns.Write(tr.item);
				ns.Write(tr.position);
				ns.Write(tr.level);
				ns.Write(tr.can_move_to_storage);
			}
			_ = last;
		}
	}
}
