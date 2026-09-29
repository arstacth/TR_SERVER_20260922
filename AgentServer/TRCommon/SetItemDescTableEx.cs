using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TRCommon
{
	public class SetItemDescTableEx
	{
		private static ConcurrentDictionary<int, SetGroupItemDesc> m_setItemTable = new ConcurrentDictionary<int, SetGroupItemDesc>();

		private static ConcurrentDictionary<int, int> m_memberToGroupTable = new ConcurrentDictionary<int, int>();

		private static Dictionary<int, Dictionary<short, short>> m_setAttrApplyTargets = new Dictionary<int, Dictionary<short, short>>();

		private Dictionary<int, List<Tuple<ITEM_POSITION, int>>> m_wearSetItemTable = new Dictionary<int, List<Tuple<ITEM_POSITION, int>>>();

		private Dictionary<int, List<Tuple<ITEM_POSITION, int>>> m_additionalSetItemTable = new Dictionary<int, List<Tuple<ITEM_POSITION, int>>>();

		private Dictionary<int, CItemAttr> m_allItemAbilities = new Dictionary<int, CItemAttr>();

		private CSetItemCompare m_setItemCompare = new CSetItemCompare(14);

		public static void onRecvData(Dictionary<int, (short, List<SetMemberItemDesc>)> source)
		{
			m_setItemTable.Clear();
			foreach (KeyValuePair<int, (short, List<SetMemberItemDesc>)> item in source)
			{
				SetGroupItemDesc setGroupItemDesc = new SetGroupItemDesc();
				setGroupItemDesc.m_groupItemNum = item.Key;
				setGroupItemDesc.m_partsCompositeCount = item.Value.Item1;
				setGroupItemDesc.m_members = item.Value.Item2;
				m_setItemTable.TryAdd(setGroupItemDesc.m_groupItemNum, setGroupItemDesc);
				foreach (SetMemberItemDesc member in setGroupItemDesc.m_members)
				{
					m_memberToGroupTable.TryAdd(member.m_memberItemDescNum, setGroupItemDesc.m_groupItemNum);
				}
			}
		}

		public static void onRecvSetAttrApplyTargets(Dictionary<int, Dictionary<short, short>> setAttrApplyTargets)
		{
			m_setAttrApplyTargets = setAttrApplyTargets;
		}

		public bool makeSetItemAbilities(AvatarInfo avatarInfo, out Dictionary<int, bool> mapGroupItemNum, bool bCostume = false)
		{
			mapGroupItemNum = new Dictionary<int, bool>();
			m_wearSetItemTable.Clear();
			m_additionalSetItemTable.Clear();
			m_allItemAbilities.Clear();
			int num = 0;
			cpk_type cpk_type2 = 0;
			cpk_type cpk_type3 = 1;
			while ((int)cpk_type3 < 15)
			{
				cpk_type2 = avatarInfo.getItemPart(cpk_type3);
				num = ShopItemTable.getItemDescNumFromCPK(0, cpk_type3, avatarInfo.getItemPart(cpk_type3));
				if (num == -1)
				{
					num = ShopItemTable.getItemDescNumFromCPK(avatarInfo.m_character, cpk_type3, avatarInfo.getItemPart(cpk_type3));
				}
				if (m_memberToGroupTable.TryGetValue(num, out var value))
				{
					int num2 = num;
					int num3 = value;
					if (getMemberItemDescByItemNum(num3, num2, out var itemDesc))
					{
						cpk_type cpk_type4 = cpk_type3;
						if (2 == (int)cpk_type3 && 30000 <= (int)cpk_type2)
						{
							cpk_type4 = 117;
						}
						if (itemDesc.m_active > 0)
						{
							if (m_wearSetItemTable.ContainsKey(num3))
							{
								m_wearSetItemTable[num3].Add(new Tuple<ITEM_POSITION, int>(cpk_type4, num2));
							}
							else
							{
								m_wearSetItemTable.Add(num3, new List<Tuple<ITEM_POSITION, int>>
								{
									new Tuple<ITEM_POSITION, int>(cpk_type4, num2)
								});
							}
						}
						else if (m_additionalSetItemTable.ContainsKey(num3))
						{
							m_additionalSetItemTable[num3].Add(new Tuple<ITEM_POSITION, int>(cpk_type4, num2));
						}
						else
						{
							m_additionalSetItemTable.Add(num3, new List<Tuple<ITEM_POSITION, int>>
							{
								new Tuple<ITEM_POSITION, int>(cpk_type4, num2)
							});
						}
						mapGroupItemNum[num3] = false;
					}
				}
				cpk_type3 = (short)((short)cpk_type3 + 1);
			}
			foreach (KeyValuePair<int, List<Tuple<ITEM_POSITION, int>>> item in m_wearSetItemTable)
			{
				int key = item.Key;
				if (getGroupItemDescByItemNum(key, out var itemDesc2) && itemDesc2.m_partsCompositeCount == item.Value.Count)
				{
					mapGroupItemNum[key] = true;
				}
			}
			return m_wearSetItemTable.Count != 0;
		}

		public void parseSetItem(Dictionary<int, bool> compareGroupNums, bool bCostume = false)
		{
			foreach (KeyValuePair<int, List<Tuple<ITEM_POSITION, int>>> item2 in m_wearSetItemTable)
			{
				List<Tuple<ITEM_POSITION, int>> value = item2.Value;
				int key = item2.Key;
				bool flag = false;
				bool flag2 = false;
				List<int> list = new List<int>();
				if (!getGroupItemDescByItemNum(key, out var itemDesc))
				{
					continue;
				}
				if (itemDesc.m_partsCompositeCount == item2.Value.Count)
				{
					flag2 = true;
					if (m_additionalSetItemTable.TryGetValue(key, out var value2))
					{
						foreach (Tuple<ITEM_POSITION, int> item3 in value2)
						{
							flag = true;
							int item = (int)item3.Item1;
							list.Add(-item);
						}
					}
				}
				m_setItemCompare.reset();
				foreach (Tuple<ITEM_POSITION, int> item4 in value)
				{
					ITEM_POSITION iTEM_POSITION = item4.Item1;
					_ = item4.Item2;
					if ((ITEM_POSITION)117 == iTEM_POSITION)
					{
						iTEM_POSITION = ITEM_POSITION.ITEM_POSITION_PET;
					}
					m_setItemCompare.set((int)(iTEM_POSITION - 1));
				}
				List<int> complexKeys = new List<int>();
				if (!m_setItemCompare.parseComplexKeys(out complexKeys))
				{
					continue;
				}
				foreach (KeyValuePair<short, float> setItemAttr in SetItemAttrTable.getSetItemAttrList(key, complexKeys))
				{
					_addAllItemAbilities(setItemAttr.Key, setItemAttr.Value, key, compareGroupNums, bCostume);
				}
				if (flag2)
				{
					foreach (KeyValuePair<short, float> item5 in SetItemAttrTable.getSpecificItemAttributes(key, 0).m_attr)
					{
						_addAllItemAbilities(item5.Key, item5.Value, key, compareGroupNums, bCostume);
					}
				}
				if (!flag)
				{
					continue;
				}
				foreach (int item6 in list)
				{
					foreach (KeyValuePair<short, float> item7 in SetItemAttrTable.getSpecificItemAttributes(key, item6).m_attr)
					{
						_addAllItemAbilities(item7.Key, item7.Value, key, compareGroupNums, bCostume);
					}
				}
			}
		}

		public bool getAllItemAbilities(out CItemAttr abilities)
		{
			abilities = new CItemAttr();
			foreach (KeyValuePair<int, CItemAttr> allItemAbility in m_allItemAbilities)
			{
				foreach (KeyValuePair<short, float> item in allItemAbility.Value.m_attr)
				{
					abilities.m_attr[item.Key] += item.Value;
				}
			}
			return !abilities.m_attr.empty;
		}

		private static bool getGroupItemDescByItemNum(int groupItemNum, out SetGroupItemDesc itemDesc)
		{
			itemDesc = new SetGroupItemDesc();
			if (m_setItemTable.TryGetValue(groupItemNum, out var value))
			{
				itemDesc = value;
				return true;
			}
			return false;
		}

		private static bool getMemberListFromGroupNum(int groupItemNum, out List<SetMemberItemDesc> members)
		{
			members = new List<SetMemberItemDesc>();
			if (m_setItemTable.TryGetValue(groupItemNum, out var value))
			{
				members = value.m_members;
				return members.Count != 0;
			}
			return false;
		}

		private static bool getMemberItemDescByItemNum(int groupItemNum, int memberItemNum, out SetMemberItemDesc itemDesc)
		{
			itemDesc = default(SetMemberItemDesc);
			if (getMemberListFromGroupNum(groupItemNum, out var members))
			{
				foreach (SetMemberItemDesc item in members)
				{
					if (item.m_memberItemDescNum == memberItemNum)
					{
						itemDesc = item;
						return true;
					}
				}
			}
			return false;
		}

		private bool _checkApplyTarget(short itemAttr, float value, int groupItemNum, Dictionary<int, bool> compareGroupNums, bool bCostume)
		{
			if (m_setAttrApplyTargets.TryGetValue(groupItemNum, out var value2))
			{
				if (value2.TryGetValue(itemAttr, out var value3))
				{
					short num = value3;
					if (!bCostume)
					{
						if (num != 0 && num != 1 && num != 3)
						{
							return num == 4;
						}
						return true;
					}
					int num2 = 0;
					bool flag = false;
					if (compareGroupNums.TryGetValue(groupItemNum, out var value4))
					{
						num2 = groupItemNum;
						flag = value4;
					}
					switch (num)
					{
					case 4:
						if (num2 != 0)
						{
							if (groupItemNum == num2)
							{
								return !flag;
							}
							return false;
						}
						return true;
					default:
						return false;
					case 2:
					case 3:
						return true;
					}
				}
				return !bCostume;
			}
			return !bCostume;
		}

		private void _addAllItemAbilities(short attr, float value, int groupItemNum, Dictionary<int, bool> compareGroupNums, bool bCostume)
		{
			if (_checkApplyTarget(attr, value, groupItemNum, compareGroupNums, bCostume) && attr != 5000)
			{
				if (!m_allItemAbilities.ContainsKey(groupItemNum))
				{
					m_allItemAbilities.Add(groupItemNum, new CItemAttr());
				}
				m_allItemAbilities[groupItemNum].m_attr[attr] += value;
			}
		}
	}
}
