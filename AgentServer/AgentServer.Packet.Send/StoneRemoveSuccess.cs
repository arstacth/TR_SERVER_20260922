using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using NestedDictionaryLib;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class StoneRemoveSuccess : NetPacket
	{
		public StoneRemoveSuccess(int ReturnStoneNum, int ItemNum, NestedDictionary<byte, byte, int, List<ItemAttr>> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_REMOVE_ACK);
			ns.Write(1);
			ns.Write((byte)1);
			ns.Write(ReturnStoneNum);
			ns.Write(0);
			ns.Write(ItemNum);
			ns.Write(infos.Count);
			foreach (KeyValuePair<byte, NestedDictionary<byte, int, List<ItemAttr>>> info in infos)
			{
				ns.Write(info.Key);
				foreach (KeyValuePair<byte, NestedDictionary<int, List<ItemAttr>>> item in info.Value)
				{
					ns.Write(item.Key);
					foreach (KeyValuePair<int, List<ItemAttr>> item2 in item.Value)
					{
						ns.Write(item2.Key);
						ns.Write(item2.Value.Count((ItemAttr c) => c.AttrValue > 0f));
						foreach (ItemAttr item3 in item2.Value.Where((ItemAttr c) => c.AttrValue > 0f))
						{
							ns.Write(item3.Attr);
							ns.Write(item3.AttrValue);
						}
					}
				}
			}
			_ = last;
		}

		public StoneRemoveSuccess(int ReturnStoneNum, UserEnchantItem item, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_REMOVE_ACK);
			ns.Write(1);
			ns.Write(value: true);
			ns.Write(ReturnStoneNum);
			ns.Write(0);
			item.encode(ns);
			_ = last;
		}
	}
}
