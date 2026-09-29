using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class ParkUseDivination_Couple_ACK : NetPacket
	{
		public ParkUseDivination_Couple_ACK(int itemnum, int limit, long gottime, List<ItemAttr> exItemAttrInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_Divination_GET_RESULT_ASK);
			ns.Write(0);
			ns.Write(3);
			ns.Write(1);
			ns.Write(itemnum);
			ns.Write(limit);
			ns.Write(gottime);
			ns.Write(exItemAttrInfo.Count);
			foreach (ItemAttr item in exItemAttrInfo)
			{
				ns.Write(item.Attr);
				ns.Write(item.AttrValue);
			}
			_ = last;
		}

		public ParkUseDivination_Couple_ACK(Account User, Dictionary<int, ExtraAbilityInfo> m_ExtraAbilities, byte last)
		{
			ns.WriteOP(Opcodes.eServer_Divination_UPDATE_COUPLE_EXTRA_ABILITY_ACK);
			ns.Write(0);
			ns.WriteBIG5Fixed_shortSize(string.Empty);
			ns.Write(m_ExtraAbilities.Count);
			foreach (ExtraAbilityInfo value2 in m_ExtraAbilities.Values)
			{
				ns.Write(value2.iItemDescNum);
				ns.Write(value2.iLimitTime);
				ns.Write(value2.tGotTime);
				ns.Write(value2.mapAttributes.Count);
				UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo
				{
					m_iItemDescNum = value2.iItemDescNum
				};
				foreach (KeyValuePair<short, float> mapAttribute in value2.mapAttributes)
				{
					short key = mapAttribute.Key;
					float value = mapAttribute.Value;
					ns.Write(key);
					ns.Write(value);
					userItemAttrInfo.m_ItemAttr[key] = value;
				}
				User.userItemAttr.deleteItemAttr(userItemAttrInfo.m_iItemDescNum);
				User.userItemAttr.insertItemAttr(userItemAttrInfo);
				User.mixUserItemAttr();
			}
			_ = last;
		}
	}
}
