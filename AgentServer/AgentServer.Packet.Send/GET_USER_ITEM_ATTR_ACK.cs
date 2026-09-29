using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GET_USER_ITEM_ATTR_ACK : NetPacket
	{
		public GET_USER_ITEM_ATTR_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_ITEM_ATTR_ACK);
			ns.Write(0);
			User.encodeUserItemAttr(ns);
			ns.Write(User.UserItemStrengthen.Count);
			foreach (KeyValuePair<int, ItemStrengthen> item in User.UserItemStrengthen)
			{
				ns.Write(item.Key);
				ns.Write((short)10);
				ns.Write(item.Value.strengthenCount);
				ns.Write(item.Value.strengthenSlotCount);
				ns.Write(item.Value.maxStrengthenSlotNum);
				ns.Write(item.Value.strengthenSlotCount + 1);
				byte i;
				for (i = 0; i <= item.Value.strengthenSlotCount; i++)
				{
					byte value;
					bool flag = User.UserItemStrengthenSlotGroup.TryGetValue(item.Key, i, out value);
					ns.Write(i);
					if (flag)
					{
						ns.Write((byte)8);
						ns.Write(value);
						User.UserItemStrengthenSlotAttr.TryGetValue(item.Key, out var value2);
						IEnumerable<IGrouping<byte, ItemAttr>> enumerable = from w in value2
							where w.slotNum == i
							select w into g
							group g by g.attrKind;
						ns.Write(enumerable.Count());
						foreach (IGrouping<byte, ItemAttr> item2 in enumerable)
						{
							ns.Write(item2.Key);
							ns.Write(item2.Count());
							foreach (ItemAttr item3 in item2)
							{
								ns.Write(item3.Attr);
								ns.Write(item3.AttrValue);
							}
						}
					}
					else
					{
						ns.Write(flag);
					}
				}
			}
			_ = last;
		}

		public GET_USER_ITEM_ATTR_ACK(Account User, eServerResult result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_ITEM_ATTR_ACK);
			ns.Write((int)result);
			User.encodeUserItemAttr(ns);
			_ = last;
		}

		public GET_USER_ITEM_ATTR_ACK(eServerResult result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_ITEM_ATTR_ACK);
			ns.Write((int)result);
			_ = last;
		}
	}
}
