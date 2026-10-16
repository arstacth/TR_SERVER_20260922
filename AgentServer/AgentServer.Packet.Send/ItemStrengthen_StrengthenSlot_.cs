using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ItemStrengthen_StrengthenSlot_ACK : NetPacket
	{
		public ItemStrengthen_StrengthenSlot_ACK(Account User, int itemNum, short slotNum, short remainStrengthenCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_REINFORCE_ITEM_OK_ACK);
			ns.Write(0);
			ns.Write(itemNum);
			ns.Write(slotNum);
			ns.Write(User.TR);
			ns.Write(remainStrengthenCount);
			ns.Write(slotNum);
			bool flag = User.UserItemStrengthenSlotGroup.ContainsKey(itemNum, 0);
			int num = ((!flag) ? 1 : 2);
			ns.Write(num);
			for (byte b = 0; b < num; b = (byte)(b + 1))
			{
				byte slotNum2 = (byte)((!flag) ? slotNum : ((b == 1) ? 7 : b));
				byte value;
				bool flag2 = User.UserItemStrengthenSlotGroup.TryGetValue(itemNum, slotNum2, out value);
				ns.Write(slotNum2);
				if (flag2)
				{
					User.UserItemStrengthenSlotAttr.TryGetValue(itemNum, out var value2);
					ns.Write((byte)2);
					ns.Write(value);
					IEnumerable<IGrouping<byte, ItemAttr>> enumerable = from w in value2
						where w.slotNum == slotNum2
						select w into g
						group g by g.attrKind;
					ns.Write(enumerable.Count());
					foreach (IGrouping<byte, ItemAttr> item in enumerable)
					{
						ns.Write(item.Key);
						ns.Write(item.Count());
						foreach (ItemAttr item2 in item)
						{
							ns.Write(item2.Attr);
							ns.Write(item2.AttrValue);
						}
					}
				}
				else
				{
					ns.Write(flag2);
				}
			}
			_ = last;
		}
	}
	public sealed class ItemStrengthen_StrengthenSlot_Failed_ACK : NetPacket
	{
		public ItemStrengthen_StrengthenSlot_Failed_ACK(int err, int itemNum, short slotNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_REINFORCE_ITEM_FAILED_ACK);
			ns.Write(err + 1);
			ns.Write(itemNum);
			ns.Write(slotNum);
			_ = last;
		}
	}
}
