using LocalCommons.Network;
using RoomServer.Packet.Send;
using TRCommon;

namespace RoomServer.Packet
{
	public class MyRoomHandle
	{
		public static void Handle_ChangeUserActiveItems(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value) && value.isInRoom(out var room))
			{
				CActiveItems cActiveItems = new CActiveItems();
				cActiveItems.decodeActiveItems(reader);
				value.activeItem = cActiveItems;
				value.charAbilityAttrMakeAttr();
				room.BroadcastToAll(new eRoom_CHANGE_USER_ACTIVE_ITEMS(value, last));
			}
		}

		public static void Handle_ItemOnOff(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value) && value.isInRoom(out var room))
			{
				int num = reader.ReadLEInt32();
				int num2 = reader.ReadLEInt32();
				int num3 = reader.ReadLEInt32();
				bool flag = reader.ReadBoolean();
				NetItemInfo netItemInfo = new NetItemInfo();
				NetItemInfo netItemInfo2 = new NetItemInfo();
				netItemInfo.m_character = reader.ReadLEUInt16();
				netItemInfo.m_position = reader.ReadLEUInt16();
				netItemInfo.m_kind = reader.ReadLEUInt16();
				netItemInfo.m_iItemDescNum = reader.ReadLEInt32();
				netItemInfo.m_expireTime = reader.ReadLEInt64();
				netItemInfo.m_tGot = reader.ReadLEInt64();
				netItemInfo.m_count = reader.ReadLEInt32();
				netItemInfo.m_exp = reader.ReadLEInt32();
				netItemInfo.m_bHasExpireTime = reader.ReadBoolean();
				netItemInfo.m_bUsing = reader.ReadBoolean();
				netItemInfo2.m_character = reader.ReadLEUInt16();
				netItemInfo2.m_position = reader.ReadLEUInt16();
				netItemInfo2.m_kind = reader.ReadLEUInt16();
				netItemInfo2.m_iItemDescNum = reader.ReadLEInt32();
				netItemInfo2.m_expireTime = reader.ReadLEInt64();
				netItemInfo2.m_tGot = reader.ReadLEInt64();
				netItemInfo2.m_count = reader.ReadLEInt32();
				netItemInfo2.m_exp = reader.ReadLEInt32();
				netItemInfo2.m_bHasExpireTime = reader.ReadBoolean();
				netItemInfo2.m_bUsing = reader.ReadBoolean();
				if (flag && netItemInfo2.valid && !value.activeItem.HasItem(netItemInfo2.m_iItemDescNum))
				{
					value.activeItem.insertItem(netItemInfo2);
				}
				NetItemInfo onItemInfo;
				NetItemInfo offItemInfo;
				switch (num2)
				{
				case 1:
					value.activeItem.updateItemOnOff_single(num, flag, out onItemInfo, out offItemInfo);
					break;
				case 2:
					value.activeItem.updateItemOnOff_group(num, num3, flag, out offItemInfo, out onItemInfo);
					break;
				}
				value.charAbilityAttrMakeAttr();
				value.SendAsync(new ITEM_ONOFF_ACK(num, num2, num3, flag, last));
				room.BroadcastToAll(new eRoom_UPDATE_ITEM_ONOFF_INFO(value, num, num2, num3, flag, netItemInfo, netItemInfo2, last));
			}
		}
	}
}
