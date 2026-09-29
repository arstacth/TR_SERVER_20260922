using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetRoomKindAttr_ACK : NetPacket
	{
		public GetRoomKindAttr_ACK(bool flag, int roomkindid, List<ItemAttr> attr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ROOM_KIND_ATTR_ACK);
			ns.Write(flag);
			ns.Write(roomkindid);
			if (flag)
			{
				ns.Write(attr.Count);
				foreach (ItemAttr item in attr)
				{
					ns.Write(item.Attr);
					ns.Write(item.AttrValue);
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
