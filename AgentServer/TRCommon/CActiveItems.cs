using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;

namespace TRCommon
{
	public class CActiveItems : CActiveItemPropertyFunction
	{
		private ConcurrentDictionary<int, NetItemInfo> m_items;

		public bool isEmptyItems => m_items.Count == 0;

		public CActiveItems()
		{
			m_items = new ConcurrentDictionary<int, NetItemInfo>();
		}

		public void clear()
		{
			m_items.Clear();
		}

		public void fromVector(List<NetItemInfo> info)
		{
			m_items.Clear();
			m_items = new ConcurrentDictionary<int, NetItemInfo>(info.ToDictionary((NetItemInfo k) => k.m_iItemDescNum, (NetItemInfo v) => v));
		}

		public void replaceItemInfoByPosition(List<NetItemInfo> info, cpk_type iPosition)
		{
			foreach (NetItemInfo item in m_items.Values.Where((NetItemInfo kvp) => (int)kvp.m_position == (int)iPosition).ToList())
			{
				m_items.TryRemove(item.m_iItemDescNum, out var _);
			}
			foreach (NetItemInfo item2 in info)
			{
				m_items.TryAdd(item2.m_iItemDescNum, item2);
			}
		}

		public void replaceItemInfoByItemNum(Dictionary<int, NetItemInfo> info)
		{
			foreach (KeyValuePair<int, NetItemInfo> item in info)
			{
				m_items[item.Key] = item.Value;
			}
		}

		public void replaceItemInfoByItem(NetItemInfo info)
		{
			if (m_items.ContainsKey(info.m_iItemDescNum))
			{
				m_items[info.m_iItemDescNum] = info;
			}
			else
			{
				m_items.TryAdd(info.m_iItemDescNum, info);
			}
		}

		public void insertItem(NetItemInfo info)
		{
			m_items.TryAdd(info.m_iItemDescNum, info);
		}

		public void insertItemForRoom(NetItemInfo info, AdvancedAvatarInfo advacedAvatarIndo, CAvatarLock al, List<int> onoffItemList)
		{
			if (advacedAvatarIndo.isWearThisItem(info.m_position, info.m_kind) || (onoffItemList.Contains(info.m_iItemDescNum) && info.m_bUsing) || al.isExist(info.m_iItemDescNum))
			{
				insertItem(info);
			}
		}

		public List<NetItemInfo> getVector()
		{
			return m_items.Values.ToList();
		}

		public bool updateItemOnOff_single(int iItemDescNum, bool bOnOff, out NetItemInfo offItemInfo, out NetItemInfo onItemInfo)
		{
			offItemInfo = new NetItemInfo();
			onItemInfo = new NetItemInfo();
			if (m_items.ContainsKey(iItemDescNum))
			{
				if (m_items[iItemDescNum].m_bUsing != bOnOff)
				{
					if (bOnOff)
					{
						onItemInfo = m_items[iItemDescNum];
					}
					else
					{
						offItemInfo = m_items[iItemDescNum];
					}
					m_items[iItemDescNum].m_bUsing = bOnOff;
					return true;
				}
				return false;
			}
			return false;
		}

		public bool updateItemOnOff_group(int iItemDescNum, int iPosition, bool bOnOff, out NetItemInfo offItemInfo, out NetItemInfo onItemInfo)
		{
			offItemInfo = new NetItemInfo();
			onItemInfo = new NetItemInfo();
			bool flag = false;
			if (bOnOff)
			{
				if (m_items.ContainsKey(iItemDescNum) && m_items[iItemDescNum].m_bUsing != bOnOff)
				{
					onItemInfo = m_items[iItemDescNum];
					m_items[iItemDescNum].m_bUsing = true;
					flag = true;
				}
				if (flag)
				{
					foreach (KeyValuePair<int, NetItemInfo> item in m_items.Where((KeyValuePair<int, NetItemInfo> w) => w.Key != iItemDescNum && (int)w.Value.m_position == iPosition && w.Value.m_bUsing))
					{
						offItemInfo = item.Value;
						item.Value.m_bUsing = false;
					}
					return flag;
				}
				return flag;
			}
			return updateItemOnOff_single(iItemDescNum, bOnOff: false, out offItemInfo, out onItemInfo);
		}

		public bool updateAutoUseItem(Dictionary<int, int> info)
		{
			foreach (KeyValuePair<int, int> item in info)
			{
				if (m_items.ContainsKey(item.Key))
				{
					m_items[item.Key].m_count = item.Value;
				}
			}
			return true;
		}

		public bool updateAssistItem(List<NetItemInfo> info, out AvatarInfo avatarInfo)
		{
			avatarInfo = default(AvatarInfo);
			return true;
		}

		public void updateItemCount(int iItemDescNum, int iCount)
		{
			if (iCount <= 0)
			{
				m_items.TryRemove(iItemDescNum, out var _);
			}
			else if (m_items.ContainsKey(iItemDescNum))
			{
				m_items[iItemDescNum].m_count = iCount;
			}
		}

		public bool isUsingItem(int iItemDescNum)
		{
			if (m_items.ContainsKey(iItemDescNum))
			{
				return m_items[iItemDescNum].m_bUsing;
			}
			return false;
		}

		public bool HasItem(int iItemDescNum, int iItemCount = 1)
		{
			if (m_items.ContainsKey(iItemDescNum))
			{
				if (1 == iItemCount)
				{
					return true;
				}
				if (1 != iItemCount && m_items[iItemDescNum].m_count >= iItemCount)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public bool HasItem(ItemDataFromDatabase itemData, int iItemCount = 1)
		{
			if (HasItem(itemData.m_iItemDescNum, iItemCount))
			{
				return true;
			}
			if (itemData.isRentalItem)
			{
				if (itemData.isEtc(eItemEtc.eItemEtc_CLOTHES_RENTAL) && HasPosition(604))
				{
					return true;
				}
				if (itemData.isEtc(eItemEtc.eItemEtc_CLOTHES_RENTAL_PREMIUM) && HasPosition(607))
				{
					return true;
				}
			}
			return false;
		}

		public int getItemCount(int iItemDescNum)
		{
			if (m_items.ContainsKey(iItemDescNum))
			{
				return m_items[iItemDescNum].m_count;
			}
			return 0;
		}

		public bool HasPosition(cpk_type position)
		{
			return m_items.Values.Any((NetItemInfo a) => (int)a.m_position == (int)position);
		}

		public bool HasPosition(cpk_type position, cpk_type kind)
		{
			return m_items.Values.Any((NetItemInfo a) => (int)a.m_position == (int)position && (int)a.m_kind == (int)kind);
		}

		public bool IsItemON(int iItemDescNum)
		{
			return m_items.Values.Any((NetItemInfo e) => e.m_iItemDescNum == iItemDescNum && e.m_bUsing);
		}

		public bool IsItemON(cpk_type position, cpk_type kind, bool bIgnoreKind = false)
		{
			if (!bIgnoreKind)
			{
				return m_items.Values.Any((NetItemInfo e) => (int)e.m_position == (int)position && (int)e.m_kind == (int)kind && e.m_bUsing);
			}
			return m_items.Values.Any((NetItemInfo e) => (int)e.m_position == (int)position && e.m_bUsing);
		}

		public List<int> getPositions(cpk_type position)
		{
			return (from w in m_items.Values
				where (int)w.m_position == (int)position
				select w into s
				select s.m_iItemDescNum).ToList();
		}

		public List<int> getOnItemList(cpk_type position)
		{
			List<int> list = (from w in m_items.Values
				where (int)w.m_position == (int)position && w.m_bUsing
				select w into s
				select s.m_iItemDescNum).ToList();
			if (list.Count == 0 && ((int)position == 185 || (int)position == 186))
			{
				list = (from w in m_items.Values
					where (int)w.m_position == (int)position
					select w into s
					select s.m_iItemDescNum).ToList();
			}
			return list;
		}

		public List<cpk_type> getOnItemListKine(cpk_type position)
		{
			List<cpk_type> list = new List<cpk_type>();
			foreach (NetItemInfo item in m_items.Values.Where((NetItemInfo w) => (int)w.m_position == (int)position && w.m_bUsing))
			{
				list.Add(item.m_kind);
			}
			return list;
		}

		public List<cpk_type> getKinds(cpk_type position)
		{
			List<cpk_type> list = new List<cpk_type>();
			foreach (NetItemInfo item in m_items.Values.Where((NetItemInfo w) => (int)w.m_position == (int)position))
			{
				list.Add(item.m_kind);
			}
			return list;
		}

		public void encodeActiveItems(PacketWriter encoder)
		{
			encoder.Write(value: false);
			encoder.Write((ushort)0);
			encoder.Write((ushort)m_items.Count);
			foreach (NetItemInfo value in m_items.Values)
			{
				encoder.Write(9206936);
				encoder.Write(value.m_character);
				encoder.Write(value.m_position);
				encoder.Write(value.m_kind);
				encoder.Write(value.m_iItemDescNum);
				encoder.Write(value.m_expireTime);
				encoder.Write(value.m_tGot);
				encoder.Write(value.m_count);
				encoder.Write(value.m_exp);
				encoder.Write(value.m_bHasExpireTime);
				encoder.Write(value.m_bUsing);
			}
			encoder.Write((ushort)0);
		}

		public void encodeActiveItemsForRoom(PacketWriter encoder, AdvancedAvatarInfo advacedAvatarIndo, CAvatarLock al)
		{
			encoder.Write(value: false);
			encoder.Write((ushort)0);
			List<NetItemInfo> list = new List<NetItemInfo>();
			if (m_items != null && !object.ReferenceEquals(advacedAvatarIndo, null))
			{
				foreach (NetItemInfo value in m_items.Values)
				{
					if (value == null)
					{
						continue;
					}
					bool onOff = ShopItemTable.getOnOffItemList != null && ShopItemTable.getOnOffItemList.Contains(value.m_iItemDescNum) && value.m_bUsing;
					bool locked = al != null && al.isExist(value.m_iItemDescNum);
					if (advacedAvatarIndo.isWearThisItem(value.m_position, value.m_kind) || onOff || locked)
					{
						list.Add(value);
					}
				}
			}
			encoder.Write((ushort)list.Count);
			foreach (NetItemInfo item in list)
			{
				encoder.Write(item.m_character);
				encoder.Write(item.m_position);
				encoder.Write(item.m_kind);
				encoder.Write(item.m_iItemDescNum);
				encoder.Write(item.m_expireTime);
				encoder.Write(item.m_tGot);
				encoder.Write(item.m_count);
				encoder.Write(item.m_exp);
				encoder.Write(item.m_bHasExpireTime);
				encoder.Write(item.m_bUsing);
			}
			encoder.Write((ushort)0);
		}

		public void decodeActiveItems(PacketReader decoder)
		{
			bool flag = decoder.ReadBoolean();
			ushort num = decoder.ReadLEUInt16();
			ushort num2 = decoder.ReadLEUInt16();
			if (num == 0)
			{
				m_items.Clear();
			}
			for (int i = 0; i < num2; i++)
			{
				NetItemInfo netItemInfo = new NetItemInfo();
				netItemInfo.m_character = decoder.ReadLEUInt16();
				netItemInfo.m_position = decoder.ReadLEUInt16();
				netItemInfo.m_kind = decoder.ReadLEUInt16();
				netItemInfo.m_iItemDescNum = decoder.ReadLEInt32();
				netItemInfo.m_expireTime = decoder.ReadLEInt64();
				netItemInfo.m_tGot = decoder.ReadLEInt64();
				netItemInfo.m_count = decoder.ReadLEInt32();
				netItemInfo.m_exp = decoder.ReadLEInt32();
				netItemInfo.m_bHasExpireTime = decoder.ReadBoolean();
				netItemInfo.m_bUsing = decoder.ReadBoolean();
				m_items.TryAdd(netItemInfo.m_iItemDescNum, netItemInfo);
			}
			if (!flag)
			{
				num2 = decoder.ReadLEUInt16();
			}
		}

		public bool deleteItem(int iItemDescNum)
		{
			m_items.TryRemove(iItemDescNum, out var _);
			return true;
		}

		public void deleteItem(cpk_type position)
		{
			foreach (NetItemInfo item in m_items.Values.Where((NetItemInfo kvp) => (int)kvp.m_position == (int)position).ToList())
			{
				m_items.TryRemove(item.m_iItemDescNum, out var _);
			}
		}

		public void deleteItem(List<int> items)
		{
			foreach (int item in items)
			{
				m_items.TryRemove(item, out var _);
			}
		}

		public void get(List<int> items, out CActiveItems activeItems)
		{
			activeItems = new CActiveItems();
			foreach (int item in items)
			{
				if (m_items.TryGetValue(item, out var value))
				{
					activeItems.insertItem(value);
				}
			}
		}

		public int useItem(int iItemDescNum)
		{
			if (m_items.ContainsKey(iItemDescNum))
			{
				if (--m_items[iItemDescNum].m_count <= 0)
				{
					m_items.TryRemove(iItemDescNum, out var _);
					return 0;
				}
				return m_items[iItemDescNum].m_count;
			}
			return -1;
		}

		public void getWithoutPosition(out List<NetItemInfo> info, eFuncItemPosition position)
		{
			info = m_items.Values.Where((NetItemInfo w) => (eFuncItemPosition)w.m_position != position).ToList();
		}

		public List<cpk_type> getOnItemListItemAttr(eItemAttr itemAttr)
		{
			List<cpk_type> list = new List<cpk_type>();
			foreach (KeyValuePair<int, NetItemInfo> item in m_items)
			{
				if (ItemAttrTable.getItemAttrFromItemDescNum(item.Key, out var it) && it.m_attr[(short)itemAttr] > 0f && item.Value.m_bUsing)
				{
					list.Add(item.Value.m_kind);
				}
			}
			return list;
		}

		public void getPropertyCheckSource(CUserItemAttrManager userItemAttr, cpk_type character, out List<CPropertyCheckSource> vecPropertyCheckSource)
		{
			vecPropertyCheckSource = new List<CPropertyCheckSource>();
			foreach (NetItemInfo value in m_items.Values)
			{
				if (((int)value.m_character == 0 || (int)value.m_character == (int)character) && (eFuncItemPosition)value.m_position > eFuncItemPosition.eFuncItemPosition_NONE && value.m_bUsing && !NetCommonFunc.isTopBodyWearItemPosition(value.m_position) && (eFuncItemPosition)value.m_position != eFuncItemPosition.eFuncItemPosition_ANIMALRACING_RIDABLE_PET && (eFuncItemPosition)value.m_position != eFuncItemPosition.eFuncItemPosition_ITEM_TRANSFORM_ITEM)
				{
					if (value.m_bHasExpireTime)
					{
						_pushProperty(value.m_iItemDescNum, value.m_position, userItemAttr, ref vecPropertyCheckSource);
					}
					else if (value.m_count > 0)
					{
						_pushProperty(value.m_iItemDescNum, value.m_position, userItemAttr, ref vecPropertyCheckSource);
					}
				}
			}
		}
	}
}
