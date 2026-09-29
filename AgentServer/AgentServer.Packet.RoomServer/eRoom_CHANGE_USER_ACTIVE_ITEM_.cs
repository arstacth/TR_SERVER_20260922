using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_CHANGE_USER_ACTIVE_ITEM_ONE : NetPacket
	{
		public eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(Account User, CActiveItems m_activeItems, Dictionary<int, ExtraAbilityInfo> m_ExtraAbilities, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEM_ONE);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write((byte)0);
			m_activeItems.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			CUserItemAttrManager cUserItemAttrManager = new CUserItemAttrManager();
			foreach (KeyValuePair<int, ExtraAbilityInfo> m_ExtraAbility in m_ExtraAbilities)
			{
				ExtraAbilityInfo value = m_ExtraAbility.Value;
				UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo();
				foreach (KeyValuePair<short, float> mapAttribute in value.mapAttributes)
				{
					userItemAttrInfo.m_iItemDescNum = value.iItemDescNum;
					userItemAttrInfo.m_ItemAttr[mapAttribute.Key] = mapAttribute.Value;
				}
				cUserItemAttrManager.insertItemAttr(userItemAttrInfo);
			}
			cUserItemAttrManager.encodeUserItemAttr(ns);
			ns.Write(last);
		}
	}
}
