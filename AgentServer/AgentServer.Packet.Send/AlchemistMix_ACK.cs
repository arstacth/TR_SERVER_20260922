using System;
using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistMix_ACK : NetPacket
	{
		public AlchemistMix_ACK(int resultitem, List<ItemAttr> Attrs, Tuple<short, short, short> dwmCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_MIX_ACK);
			ns.Write(0);
			ns.Write(resultitem);
			ns.Write(Attrs.Count);
			foreach (ItemAttr Attr in Attrs)
			{
				ns.Write(Attr.Attr);
				ns.Write(Attr.AttrValue);
			}
			ns.Write(dwmCount.Item1);
			ns.Write(dwmCount.Item2);
			ns.Write(dwmCount.Item3);
			_ = last;
		}

		public AlchemistMix_ACK(UserItemAttrInfo itemAttrInfo, Tuple<short, short, short> dwmCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_MIX_ACK);
			ns.Write(0);
			ns.Write(itemAttrInfo.m_iItemDescNum);
			ns.Write(itemAttrInfo.m_ItemAttr.Count);
			foreach (KeyValuePair<short, float> item in itemAttrInfo.m_ItemAttr)
			{
				ns.Write(item.Key);
				ns.Write(item.Value);
			}
			ns.Write(dwmCount.Item1);
			ns.Write(dwmCount.Item2);
			ns.Write(dwmCount.Item3);
			_ = last;
		}
	}
}
