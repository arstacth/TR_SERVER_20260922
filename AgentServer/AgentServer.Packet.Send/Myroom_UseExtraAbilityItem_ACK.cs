using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_UseExtraAbilityItem_ACK : NetPacket
	{
		public Myroom_UseExtraAbilityItem_ACK(int ItemNum, ExtraAbilityItemInfo exItemInfo, ExtraAbilityItemAttrInfo exItemAttrInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_USE_EXTRA_ABILITY_ITEM_ACK);
			ns.Write(exItemInfo.divinationType);
			ns.Write(ItemNum);
			ns.Write(exItemInfo.remainItemCount);
			ns.Write(1);
			ns.Write(exItemAttrInfo.itemnum);
			ns.Write(exItemAttrInfo.limit);
			ns.Write(exItemAttrInfo.gottime);
			ns.Write(exItemAttrInfo.attrlist.Count);
			foreach (ItemAttr item in exItemAttrInfo.attrlist)
			{
				ns.Write(item.Attr);
				ns.Write(item.AttrValue);
			}
			_ = last;
		}

		public Myroom_UseExtraAbilityItem_ACK(Account User, int divinationType, int iItemNum, int iRemainItemCount, Dictionary<int, ExtraAbilityInfo> m_ExtraAbilities, byte last)
		{
			ns.WriteOP(Opcodes.eServer_USE_EXTRA_ABILITY_ITEM_ACK);
			ns.Write(divinationType);
			ns.Write(iItemNum);
			ns.Write(iRemainItemCount);
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
					if (!userItemAttrInfo.m_ItemAttr.ContainsKey(key))
					{
						userItemAttrInfo.m_ItemAttr.Add(key, value);
					}
				}
				User.userItemAttr.deleteItemAttr(userItemAttrInfo.m_iItemDescNum);
				User.userItemAttr.insertItemAttr(userItemAttrInfo);
				User.mixUserItemAttr();
			}
			_ = last;
		}
	}
}
