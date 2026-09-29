using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DyedItemList : NetPacket
	{
		public DyedItemList(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_DYEING_DYED_ITEM_LIST_ACK);
			ns.Write(0);
			if (User.DyedItemList.TryGetValue(1, out var value))
			{
				ns.Write(value.Count);
				foreach (KeyValuePair<int, UserItemDyeing> item in value)
				{
					ns.Write(item.Key);
					ns.Write(item.Value.DyeingPart);
					ns.Write(item.Value.Color1, 0, 3);
					ns.Write(item.Value.Color2, 0, 3);
					ns.Write(item.Value.Color3, 0, 3);
				}
			}
			else
			{
				ns.Write(0);
			}
			if (User.DyedItemList.TryGetValue(2, out var value2))
			{
				ns.Write(value2.Count);
				foreach (KeyValuePair<int, UserItemDyeing> item2 in value2)
				{
					ns.Write(item2.Key);
					ns.Write(item2.Value.DyeingPart);
					ns.Write(item2.Value.Color1, 0, 3);
					ns.Write(item2.Value.Color2, 0, 3);
					ns.Write(item2.Value.Color3, 0, 3);
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
