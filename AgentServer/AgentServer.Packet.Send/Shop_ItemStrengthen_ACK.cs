using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shop_ItemStrengthen_ACK : NetPacket
	{
		public Shop_ItemStrengthen_ACK(Account User, int itemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_ITEM_ADD_NOTIFY_ACK);
			ns.Write(User.UserItemStrengthen.Count((KeyValuePair<int, ItemStrengthen> c) => c.Key == itemNum));
			foreach (KeyValuePair<int, ItemStrengthen> item in User.UserItemStrengthen.Where((KeyValuePair<int, ItemStrengthen> w) => w.Key == itemNum))
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
						ns.Write(value2.Count((ItemAttr c) => c.slotNum == i));
						foreach (ItemAttr item2 in value2.Where((ItemAttr w) => w.slotNum == i))
						{
							ns.Write(item2.attrKind);
							ns.Write(1);
							ns.Write(item2.Attr);
							ns.Write(item2.AttrValue);
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
	}
}
