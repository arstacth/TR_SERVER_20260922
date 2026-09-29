using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using NestedDictionaryLib;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class EnchantItemInfo : NetPacket
	{
		public EnchantItemInfo(int ItemNum, NestedDictionary<int, byte, byte, int, List<ItemAttr>> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_GET_MY_ITEM_ACK);
			ns.Write(1);
			ns.Write(ItemNum);
			ns.Write(infos.Count);
			if (infos.Count != 0)
			{
				foreach (KeyValuePair<int, NestedDictionary<byte, byte, int, List<ItemAttr>>> info in infos)
				{
					ns.Write(info.Key);
					ns.Write(info.Value.Count);
					foreach (KeyValuePair<byte, NestedDictionary<byte, int, List<ItemAttr>>> item in info.Value)
					{
						ns.Write(item.Key);
						foreach (KeyValuePair<byte, NestedDictionary<int, List<ItemAttr>>> item2 in item.Value)
						{
							ns.Write(item2.Key);
							foreach (KeyValuePair<int, List<ItemAttr>> item3 in item2.Value)
							{
								ns.Write(item3.Key);
								ns.Write(item3.Value.Count((ItemAttr c) => c.AttrValue > 0f));
								foreach (ItemAttr item4 in item3.Value.Where((ItemAttr c) => c.AttrValue > 0f))
								{
									ns.Write(item4.Attr);
									ns.Write(item4.AttrValue);
								}
							}
						}
					}
				}
			}
			_ = last;
		}

		public EnchantItemInfo(int ItemNum, Dictionary<int, UserEnchantItem> m_list, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_GET_MY_ITEM_ACK);
			ns.Write(1);
			ns.Write(ItemNum);
			ns.Write(m_list.Count);
			foreach (KeyValuePair<int, UserEnchantItem> item in m_list)
			{
				item.Value.encode(ns);
			}
			_ = last;
		}
	}
}
