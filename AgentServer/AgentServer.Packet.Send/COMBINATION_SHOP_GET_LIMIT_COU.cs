using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using NestedDictionaryLib;

namespace AgentServer.Packet.Send
{
	public sealed class COMBINATION_SHOP_GET_LIMIT_COUNT_INFO_ACK : NetPacket
	{
		public COMBINATION_SHOP_GET_LIMIT_COUNT_INFO_ACK(int systemType, int exchangeID, NestedDictionary<int, int, Tuple<int, int>> ExchangeCountList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMBINATION_SHOP_GET__LIMIT_COUNT_INFO_ACK);
			ns.Write(systemType);
			ns.Write(0);
			if (ExchangeCountList != null && ExchangeCountList.ContainsKey(systemType))
			{
				if (exchangeID != 0)
				{
					ns.Write(ExchangeCountList[systemType].Count((KeyValuePair<int, Tuple<int, int>> c) => c.Key == exchangeID));
					foreach (KeyValuePair<int, Tuple<int, int>> item in ExchangeCountList[systemType].Where((KeyValuePair<int, Tuple<int, int>> w) => w.Key == exchangeID))
					{
						ns.Write(item.Key);
						ns.Write(item.Value.Item2);
					}
				}
				else
				{
					ns.Write(ExchangeCountList[systemType].Count);
					foreach (KeyValuePair<int, Tuple<int, int>> item2 in ExchangeCountList[systemType])
					{
						ns.Write(item2.Key);
						ns.Write(item2.Value.Item2);
					}
				}
			}
			else
			{
				ns.Write(0);
			}
			_ = last;
		}
	}
}
