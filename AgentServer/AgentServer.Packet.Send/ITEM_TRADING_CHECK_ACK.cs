using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ITEM_TRADING_CHECK_ACK : NetPacket
	{
		public ITEM_TRADING_CHECK_ACK(List<ItemTradeInfo> ItemTradeInfo, Dictionary<int, int> ticket_count, int ItemNum, eTRADE_CHECK_RESULT result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_TRADING_CHECK);
			if (ItemTradeInfo != null)
			{
				ns.Write(ItemTradeInfo.Count);
				foreach (ItemTradeInfo item in ItemTradeInfo)
				{
					ns.Write(item.m_ticket);
					ns.Write(item.m_result_count);
					ns.Write(item.m_min_unduplicated_item);
					ns.Write(item.m_position2ratio.Count);
					foreach (KeyValuePair<int, float> item2 in item.m_position2ratio)
					{
						ns.Write(item2.Key);
						ns.Write(item2.Value);
					}
					ns.Write(item.m_levelratio.Count);
					foreach (KeyValuePair<int, float> item3 in item.m_levelratio)
					{
						ns.Write(item3.Key);
						ns.Write(item3.Value);
					}
				}
			}
			else
			{
				ns.Write(0);
			}
			ns.Write(ticket_count.Count);
			foreach (KeyValuePair<int, int> item4 in ticket_count)
			{
				ns.Write(item4.Key);
				ns.Write(item4.Value);
			}
			ns.Write(ItemNum);
			ns.Write((byte)result);
			_ = last;
		}
	}
}
