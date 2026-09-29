using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_CharacterStatConfirm_ACK : NetPacket
	{
		public Myroom_CharacterStatConfirm_ACK(int ItemNum, List<ItemAttr> newAttrList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_CONFIRM_ACK);
			ns.Write(ItemNum);
			ns.Write(newAttrList.Count);
			foreach (ItemAttr newAttr in newAttrList)
			{
				ns.Write(newAttr.Attr);
				ns.Write(newAttr.AttrValue);
			}
			_ = last;
		}

		public Myroom_CharacterStatConfirm_ACK(UserItemAttrInfo m_CharAttr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_CONFIRM_ACK);
			ns.Write(m_CharAttr.m_iItemDescNum);
			ns.Write(m_CharAttr.m_ItemAttr.Count);
			foreach (KeyValuePair<short, float> item in m_CharAttr.m_ItemAttr)
			{
				ns.Write(item.Key);
				ns.Write(item.Value);
			}
			_ = last;
		}
	}
}
