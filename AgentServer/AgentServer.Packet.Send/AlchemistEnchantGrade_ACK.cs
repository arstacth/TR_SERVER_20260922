using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistEnchantGrade_ACK : NetPacket
	{
		public AlchemistEnchantGrade_ACK(int resultitem, List<ItemAttr> Attrs, bool bSuccess, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_ENCHANT_GRADE_ACK);
			ns.Write(0);
			ns.Write(bSuccess);
			ns.Write(resultitem);
			ns.Write(Attrs.Count);
			foreach (ItemAttr Attr in Attrs)
			{
				ns.Write(Attr.Attr);
				ns.Write(Attr.AttrValue);
			}
			_ = last;
		}

		public AlchemistEnchantGrade_ACK(UserItemAttrInfo itemAttrInfo, bool bSuccess, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_ENCHANT_GRADE_ACK);
			ns.Write(0);
			ns.Write(bSuccess);
			ns.Write(itemAttrInfo.m_iItemDescNum);
			ns.Write(itemAttrInfo.m_ItemAttr.Count);
			foreach (KeyValuePair<short, float> item in itemAttrInfo.m_ItemAttr)
			{
				ns.Write(item.Key);
				ns.Write(item.Value);
			}
			_ = last;
		}
	}
}
