using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Item;
using TRCommon;

namespace RoomServer.Packet
{
	public class ItemHandle
	{
		public static void UpdateAvatarInfo(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				List<UserItemDyeing> list = new List<UserItemDyeing>();
				list.AddRange(Enumerable.Repeat(new UserItemDyeing(), 24));
				AvatarInfo realInfo = value.advancedAvatarInfo.m_realAvatarInfo;
				for (int i = 0; i < 15; i++)
				{
					realInfo.SetWear(i, reader.ReadLEUInt16());
				}
				value.advancedAvatarInfo.m_realAvatarInfo = realInfo;
				for (int j = 0; j < 12; j++)
				{
					UserItemDyeing userItemDyeing2 = (list[j] = new UserItemDyeing
					{
						DyeingPart = reader.ReadByte(),
						Color1 = reader.ReadByteArray(3),
						Color2 = reader.ReadByteArray(3),
						Color3 = reader.ReadByteArray(3)
					});
				}
				AvatarInfo costumeInfo = value.advancedAvatarInfo.m_costumeAvatarInfo;
				for (int k = 0; k < 15; k++)
				{
					costumeInfo.SetWear(k, reader.ReadLEUInt16());
				}
				value.advancedAvatarInfo.m_costumeAvatarInfo = costumeInfo;
				for (int l = 12; l < 24; l++)
				{
					UserItemDyeing userItemDyeing4 = (list[l] = new UserItemDyeing
					{
						DyeingPart = reader.ReadByte(),
						Color1 = reader.ReadByteArray(3),
						Color2 = reader.ReadByteArray(3),
						Color3 = reader.ReadByteArray(3)
					});
				}
				value.advancedAvatarInfo.m_bIsUseCostume = reader.ReadBoolean();
				reader.Offset++;
				value.AvatarItemDyeing = list;
				value.activeItem.decodeActiveItems(reader);
				value.charAbilityAttrMakeAttr();
				reader.ReadBoolean();
				if (value.isInRoom(out var room))
				{
					room.BroadcastToAll(new eRoom_UPDATE_AVATAR_INFO(value, last));
				}
			}
		}

		public static void ActiveFuncItem_Timeout(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				int num = reader.ReadLEInt32();
				List<int> list = new List<int>();
				for (int i = 0; i < num; i++)
				{
					list.Add(reader.ReadLEInt32());
				}
				_onRecvItemTimeout(value, list, last);
			}
		}

		public static void Change_UserItemAttr(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				value.userItemAttr.clear();
				value.userItemAttr.decodeUserItemAttr(reader);
				if (value.isInRoom(out var room))
				{
					room.BroadcastToAll(new eRoom_CHANGE_USER_ITEM_ATTR(value, last));
				}
			}
		}

		public static void Change_UserActiveItemOne(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			byte byChangeFlag = reader.ReadByte();
			CActiveItems cActiveItems = new CActiveItems();
			cActiveItems.decodeActiveItems(reader);
			foreach (NetItemInfo item in cActiveItems.getVector())
			{
				value.activeItem.replaceItemInfoByItem(item);
			}
			CUserItemAttrManager cUserItemAttrManager = new CUserItemAttrManager();
			cUserItemAttrManager.decodeUserItemAttr(reader);
			foreach (KeyValuePair<int, UserItemAttrInfo> getItemAttr in cUserItemAttrManager.getItemAttrList)
			{
				value.userItemAttr.deleteItemAttr(getItemAttr.Key);
				value.userItemAttr.insertItemAttr(getItemAttr.Value);
			}
			value.charAbilityAttrMakeAttr();
			if (value.isInRoom(out var room))
			{
				if (room.RoomKindID == 74 || room.RoomKindID == 75 || room.RoomKindID == 76)
				{
					value.SendAsync(new eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(value, byChangeFlag, cActiveItems, cUserItemAttrManager, last));
				}
				else
				{
					room.BroadcastToAll(new eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(value, byChangeFlag, cActiveItems, cUserItemAttrManager, last));
				}
			}
		}

		private static void _onRecvItemTimeout(Account roomUserData, List<int> deleteItems, byte last = 1)
		{
			roomUserData.activeItem.get(deleteItems, out var activeItems);
			roomUserData.activeItem.deleteItem(deleteItems);
			foreach (NetItemInfo item in activeItems.getVector())
			{
				roomUserData.userItemAttr.deleteItemAttr(item.m_iItemDescNum);
			}
			if (roomUserData.isInRoom(out var room))
			{
				room.BroadcastToAll(new eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT(roomUserData, activeItems, last));
			}
		}
	}
}
