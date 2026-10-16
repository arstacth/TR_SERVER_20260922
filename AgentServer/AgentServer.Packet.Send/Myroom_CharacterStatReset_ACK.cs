using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_CharacterStatReset_ACK : NetPacket
	{
		public Myroom_CharacterStatReset_ACK(int statNum, int ItemNum, List<ItemAttr> newAttrList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_RESET_ACK);
			ns.Write(statNum);
			ns.Write(ItemNum);
			ns.Write(newAttrList.Count);
			foreach (ItemAttr newAttr in newAttrList)
			{
				ns.Write(newAttr.Attr);
				ns.Write(newAttr.AttrValue);
			}
			_ = last;
		}

		public Myroom_CharacterStatReset_ACK(int statNum, UserItemAttrInfo m_CharAttr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_RESET_ACK);
			ns.Write(statNum);
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
