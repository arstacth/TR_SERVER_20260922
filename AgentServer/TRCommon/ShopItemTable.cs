using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog;

namespace TRCommon
{
	public static class ShopItemTable
	{
		public static bool isRecvItemListFromDB = false;

		private static SortedSet<int> m_manCharNums = new SortedSet<int>();

		private static SortedSet<int> m_womanCharNums = new SortedSet<int>();

		private static SortedSet<int> m_permanentEquipmentItems = new SortedSet<int>();

		private static SortedSet<int> m_permanentItems = new SortedSet<int>();

		private static SortedSet<int> m_durableItems = new SortedSet<int>();

		private static SortedSet<int> m_onoffItemList = new SortedSet<int>();

		private static Dictionary<short, float> m_limit = new Dictionary<short, float>();

		private static Dictionary<int, FarmItemDataFromDatabase> m_farmItemPositionMap = new Dictionary<int, FarmItemDataFromDatabase>();

		private static Dictionary<int, CNetShuItemInfo> m_shuItemList = new Dictionary<int, CNetShuItemInfo>();

		private static ConcurrentDictionary<(int c, int p, int k), int> m_CPKToItemDescNum = new ConcurrentDictionary<(int, int, int), int>();

		private static ConcurrentDictionary<int, ItemBillingContents> m_itemNumToItemBillingContents = new ConcurrentDictionary<int, ItemBillingContents>();

		private static ConcurrentDictionary<int, (int, int, int)> m_itemDescNumToItemQuery = new ConcurrentDictionary<int, (int, int, int)>();

		private static ConcurrentDictionary<int, ItemDataFromDatabase> m_itemDescNumToItemData = new ConcurrentDictionary<int, ItemDataFromDatabase>();

		private static Dictionary<int, int> m_cardDescNumToGetRate = new Dictionary<int, int>();

		private static ConcurrentDictionary<int, CItemInfo_RealTime> m_realtime_item = new ConcurrentDictionary<int, CItemInfo_RealTime>();

		public static Dictionary<short, float> getAttrLimit => m_limit;

		public static SortedSet<int> getOnOffItemList => m_onoffItemList;

		public static void onRecvItemDataFromDB(Dictionary<int, ItemDataFromDatabase> itemDatas, Dictionary<int, FarmItemDataFromDatabase> farmItemMap, Dictionary<int, CNetShuItemInfo> shuItemList)
		{
			try
			{
				Dictionary<int, CItemAttr> dictionary = new Dictionary<int, CItemAttr>();
				foreach (KeyValuePair<int, ItemDataFromDatabase> itemData in itemDatas)
				{
					ItemDataFromDatabase value = itemData.Value;
					if (value.m_mapAttr.TryGetValue(79, out var value2))
					{
						if (1 == (int)value2)
						{
							m_manCharNums.Add(value.m_iCharacter);
						}
						else if (2 == (int)value2)
						{
							m_womanCharNums.Add(value.m_iCharacter);
						}
					}
					if (1 == value.m_iType && value.isEquipItem)
					{
						m_permanentEquipmentItems.Add(value.m_iItemDescNum);
					}
					if (1 == value.m_iType && !value.m_bCanStack)
					{
						m_permanentItems.Add(value.m_iItemDescNum);
					}
					if (2 == value.m_iType && value.m_bNotDeleteWhenExpired)
					{
						m_durableItems.Add(value.m_iItemDescNum);
					}
					if (0 < value.m_iOnOffType || NetCommonFunc.isUsableItemInRoom(value.m_iPosition))
					{
						m_onoffItemList.Add(value.m_iItemDescNum);
					}
					m_itemDescNumToItemData[value.m_iItemDescNum] = value;
					m_cardDescNumToGetRate[value.m_iItemDescNum] = value.m_iGotRateKind;
					(int, int, int) tuple = (value.m_iCharacter, value.m_iPosition, value.m_iItemKind);
					m_CPKToItemDescNum[tuple] = value.m_iItemDescNum;
					m_itemDescNumToItemQuery[value.m_iItemDescNum] = tuple;
					m_itemNumToItemBillingContents[value.m_iItemDescNum] = new ItemBillingContents(value.m_iItemDescNum, value.m_iBillingItemID, value.m_iTRPrice, value.m_iCashPrice, value.m_iFarmPointPrice, value.m_iGuildPointPrice, value.m_iContributionPointPrice, value.m_strItemName, eShopPriceType.eShopPriceType_UNKNOWN);
					CItemAttr cItemAttr = new CItemAttr();
					foreach (KeyValuePair<short, float> item in value.m_mapAttr)
					{
						short key = item.Key;
						float value3 = item.Value;
						if (key < 7016)
						{
							cItemAttr.m_attr[key] = value3;
						}
					}
					dictionary[value.m_iItemDescNum] = cItemAttr;
				}
				ItemAttrTable.onRecvItemAttr(dictionary);
				m_farmItemPositionMap = farmItemMap;
				m_shuItemList = shuItemList;
				isRecvItemListFromDB = true;
			}
			catch (Exception ex)
			{
				Log.Error("onRecvItemDataFromDB:{0}", ex.ToString());
			}
		}

		public static cpk_type getShuItemKind(int avatarItemNum)
		{
			if (m_shuItemList.TryGetValue(avatarItemNum, out var value))
			{
				return value.m_kind;
			}
			return 65535;
		}

		public static cpk_type getShuItemPosition(int avatarItemNum)
		{
			if (m_shuItemList.TryGetValue(avatarItemNum, out var value))
			{
				return value.m_position;
			}
			return 0;
		}

		public static cpk_type getShuItemCharacter(int avatarItemNum)
		{
			if (m_shuItemList.TryGetValue(avatarItemNum, out var value))
			{
				return value.m_character;
			}
			return 0;
		}

		public static int getItemDescNumFromCPK(cpk_type Character, cpk_type Position, cpk_type Kind)
		{
			cpk_type originKindNumFromCPK = TransformItemManager.getOriginKindNumFromCPK(Character, Position, Kind);
			cpk_type itemPositionByItemKind = NetCommonFunc.getItemPositionByItemKind(Position, originKindNumFromCPK);
			int result = -1;
			(cpk_type, cpk_type, cpk_type) tuple = (Character, itemPositionByItemKind, originKindNumFromCPK);
			ConcurrentDictionary<(int c, int p, int k), int> cPKToItemDescNum = m_CPKToItemDescNum;
			(cpk_type, cpk_type, cpk_type) tuple2 = tuple;
			if (cPKToItemDescNum.ContainsKey((tuple2.Item1, tuple2.Item2, tuple2.Item3)))
			{
				ConcurrentDictionary<(int c, int p, int k), int> cPKToItemDescNum2 = m_CPKToItemDescNum;
				tuple2 = tuple;
				result = cPKToItemDescNum2[(tuple2.Item1, tuple2.Item2, tuple2.Item3)];
			}
			else
			{
				tuple = (0, itemPositionByItemKind, originKindNumFromCPK);
				ConcurrentDictionary<(int c, int p, int k), int> cPKToItemDescNum3 = m_CPKToItemDescNum;
				tuple2 = tuple;
				if (cPKToItemDescNum3.ContainsKey((tuple2.Item1, tuple2.Item2, tuple2.Item3)))
				{
					ConcurrentDictionary<(int c, int p, int k), int> cPKToItemDescNum4 = m_CPKToItemDescNum;
					tuple2 = tuple;
					result = cPKToItemDescNum4[(tuple2.Item1, tuple2.Item2, tuple2.Item3)];
				}
			}
			return result;
		}

		public static bool HasCpk(int character, int position, int kind)
		{
			if (kind <= 0 || kind == 65535)
			{
				return false;
			}
			int desc = getItemDescNumFromCPK((ushort)character, (ushort)position, (ushort)kind);
			return desc > 0 && m_itemDescNumToItemData != null && m_itemDescNumToItemData.ContainsKey(desc);
		}

		public static ushort FirstValidWearKind(int character, int position)
		{
			_ = character;
			_ = position;
			return 2;
		}

		public static void FillMissingWearKinds(ref AvatarInfo info)
		{
			int ch = (int)info.m_character;
			if (ch == 0 || ch == 65535)
			{
				return;
			}
			if (info.GetWear(0) == 0 || info.GetWear(0) == 65535)
			{
				info.SetWear(0, (ushort)ch);
			}
			for (int p = 1; p <= 4; p++)
			{
				if (p == (int)ITEM_POSITION.ITEM_POSITION_DOWNBODY)
				{
					if (info.GetWear(p) == 65535)
					{
						info.SetWear(p, 0);
					}
					continue;
				}
				int k = info.GetWear(p);
				if (k == 0 || k == 65535)
				{
					info.SetWear(p, FirstValidWearKind(ch, p));
				}
			}
			for (int p = 5; p <= 14; p++)
			{
				if (info.GetWear(p) == 65535)
				{
					info.SetWear(p, 0);
				}
			}
		}

		public static bool isEnchantItem(int iItemNum)
		{
			if (!getItemDataFromItemDescNum(iItemNum, out var itemData))
			{
				return false;
			}
			return itemData.isEtc(eItemEtc.eItemEtc_ENCHANT_ITEM);
		}

		public static bool getRealItemDataFromCPK(cpk_type Character, cpk_type Position, cpk_type Kind, out ItemDataFromDatabase itemData)
		{
			itemData = new ItemDataFromDatabase();
			if (65535 == (int)Kind || (int)Kind == 0)
			{
				return false;
			}
			if (!getItemDataFromItemDescNum(getItemDescNumFromCPK(Character, Position, Kind), out itemData))
			{
				Log.Error("There's no item information!! itemNum : c : {0}, p : {0}, k : {0}", Character, Position, Kind);
				return false;
			}
			return true;
		}

		public static bool getItemQueryFromItemDescNum(int iItemDescNum, out (int character, int position, int kind) query)
		{
			query = (0, 0, 0);
			query.character = 65535;
			query.position = 65535;
			query.kind = 65535;
			if (!m_itemDescNumToItemQuery.TryGetValue(iItemDescNum, out var value))
			{
				Log.Error("!!!! Can't find itemDescNum : {0}", iItemDescNum);
				return false;
			}
			query = value;
			return true;
		}

		public static bool getCPKfromItemDescNum(int iItemNum, out cpk_type Character, out cpk_type Position, out cpk_type Kind)
		{
			Character = 0;
			Position = 0;
			Kind = 0;
			if (getItemQueryFromItemDescNum(iItemNum, out var query))
			{
				Character = query.Item1;
				Position = query.Item2;
				Kind = query.Item3;
				return true;
			}
			return false;
		}

		public static bool getItemBillingContentsFromBuyingItemInfo(BuyingItemInfo goodsInfo, out ItemBillingContents contents)
		{
			return getItemBillingContentsFromItemDescNum(goodsInfo.m_iItemDescNum, goodsInfo.m_ePriceType, out contents);
		}

		public static bool getItemBillingContentsFromItemDescNum(int iItemDescNum, out ItemBillingContents contents)
		{
			contents = new ItemBillingContents();
			if (!m_itemNumToItemBillingContents.TryGetValue(iItemDescNum, out var value))
			{
				return false;
			}
			contents = value;
			contents.m_paymentType = eShopPriceType.eShopPriceType_UNKNOWN;
			return true;
		}

		public static bool getItemBillingContentsFromItemDescNum(int iItemNum, eShopPriceType priceType, out ItemBillingContents contents)
		{
			if (getItemBillingContentsFromItemDescNum(iItemNum, out contents))
			{
				contents.m_paymentType = priceType;
				switch (contents.m_paymentType)
				{
				case eShopPriceType.eShopPriceType_CASH:
					contents.m_iGameMoneyPrice = 0;
					contents.m_iFarmPointPrice = 0;
					contents.m_iGuildPointPrice = 0;
					contents.m_iContributionPointPrice = 0;
					break;
				case eShopPriceType.eShopPriceType_GAMEMONEY:
					contents.m_iCashPrice = 0;
					contents.m_iFarmPointPrice = 0;
					contents.m_iGuildPointPrice = 0;
					contents.m_iContributionPointPrice = 0;
					break;
				case eShopPriceType.eShopPriceType_FARMPOINT:
					contents.m_iCashPrice = 0;
					contents.m_iGameMoneyPrice = 0;
					contents.m_iGuildPointPrice = 0;
					contents.m_iContributionPointPrice = 0;
					break;
				case eShopPriceType.eShopPriceType_GUILDPOINT:
					contents.m_iCashPrice = 0;
					contents.m_iGameMoneyPrice = 0;
					contents.m_iFarmPointPrice = 0;
					contents.m_iContributionPointPrice = 0;
					break;
				case eShopPriceType.eShopPriceType_CONTRIBUTIONPOINT:
					contents.m_iCashPrice = 0;
					contents.m_iGameMoneyPrice = 0;
					contents.m_iFarmPointPrice = 0;
					contents.m_iGuildPointPrice = 0;
					break;
				case eShopPriceType.eShopPriceType_MILEAGE:
					contents.m_iCashPrice = 0;
					contents.m_iGameMoneyPrice = 0;
					contents.m_iFarmPointPrice = 0;
					contents.m_iGuildPointPrice = 0;
					contents.m_iContributionPointPrice = 0;
					break;
				default:
					if (contents.m_iCashPrice > 0)
					{
						contents.m_paymentType = eShopPriceType.eShopPriceType_CASH;
						contents.m_iGameMoneyPrice = 0;
						contents.m_iFarmPointPrice = 0;
						contents.m_iGuildPointPrice = 0;
						contents.m_iContributionPointPrice = 0;
					}
					else if (contents.m_iGameMoneyPrice > 0)
					{
						contents.m_paymentType = eShopPriceType.eShopPriceType_GAMEMONEY;
						contents.m_iCashPrice = 0;
						contents.m_iFarmPointPrice = 0;
						contents.m_iGuildPointPrice = 0;
						contents.m_iContributionPointPrice = 0;
					}
					else if (contents.m_iFarmPointPrice > 0)
					{
						contents.m_paymentType = eShopPriceType.eShopPriceType_FARMPOINT;
						contents.m_iCashPrice = 0;
						contents.m_iGameMoneyPrice = 0;
						contents.m_iGuildPointPrice = 0;
						contents.m_iContributionPointPrice = 0;
					}
					else if (contents.m_iGuildPointPrice > 0)
					{
						contents.m_paymentType = eShopPriceType.eShopPriceType_GUILDPOINT;
						contents.m_iCashPrice = 0;
						contents.m_iGameMoneyPrice = 0;
						contents.m_iFarmPointPrice = 0;
						contents.m_iContributionPointPrice = 0;
					}
					else if (contents.m_iContributionPointPrice > 0)
					{
						contents.m_paymentType = eShopPriceType.eShopPriceType_CONTRIBUTIONPOINT;
						contents.m_iCashPrice = 0;
						contents.m_iGameMoneyPrice = 0;
						contents.m_iFarmPointPrice = 0;
						contents.m_iGuildPointPrice = 0;
					}
					break;
				}
				return true;
			}
			Log.Error("Can't find getItemBillingContentsFromItemDescNum - {0}, {1}", iItemNum, priceType);
			return false;
		}

		public static bool getItemDataFromItemDescNum(int iItemDescNum, out ItemDataFromDatabase itemData)
		{
			itemData = new ItemDataFromDatabase();
			if (!m_itemDescNumToItemData.ContainsKey(iItemDescNum))
			{
				return false;
			}
			itemData = m_itemDescNumToItemData[iItemDescNum];
			if (m_realtime_item.ContainsKey(iItemDescNum))
			{
				itemData.m_bPurchasable = m_realtime_item[iItemDescNum].is_purchasable;
				itemData.m_bShowInShop = m_realtime_item[iItemDescNum].is_showshop;
				itemData.m_iEtc = m_realtime_item[iItemDescNum].etc;
			}
			return true;
		}

		public static bool getRealItemDataFromItemDescNum(int iItemDescNum, out ItemDataFromDatabase itemData)
		{
			itemData = new ItemDataFromDatabase();
			if (m_itemDescNumToItemData.TryGetValue(iItemDescNum, out var value))
			{
				if (value.m_iSupplyItemDescNum == 0 || 4 == value.m_iType)
				{
					itemData = value;
					if (m_realtime_item.TryGetValue(iItemDescNum, out var value2))
					{
						itemData.m_bPurchasable = value2.is_purchasable;
						itemData.m_bShowInShop = value2.is_showshop;
						itemData.m_iEtc = value2.etc;
					}
					return true;
				}
				if (m_itemDescNumToItemData.TryGetValue(value.m_iSupplyItemDescNum, out var value3))
				{
					itemData = value3;
					if (m_realtime_item.TryGetValue(iItemDescNum, out var value4))
					{
						itemData.m_bPurchasable = value4.is_purchasable;
						itemData.m_bShowInShop = value4.is_showshop;
						itemData.m_iEtc = value4.etc;
					}
					return true;
				}
			}
			return false;
		}

		public static bool isFarmItem(int iItemDescNum)
		{
			if (!m_itemDescNumToItemData.TryGetValue(iItemDescNum, out var value))
			{
				return false;
			}
			if (3 == value.m_iType && !m_itemDescNumToItemData.TryGetValue(value.m_iSupplyItemDescNum, out value))
			{
				return false;
			}
			if (value.m_iPosition != 1000)
			{
				return value.m_iPosition == 1080;
			}
			return true;
		}

		public static bool isShuItem(int iItemDescNum)
		{
			if (!m_itemDescNumToItemData.TryGetValue(iItemDescNum, out var value))
			{
				return false;
			}
			if (3 == value.m_iType && !m_itemDescNumToItemData.TryGetValue(value.m_iSupplyItemDescNum, out value))
			{
				return false;
			}
			return value.m_iPosition == 50000;
		}

		public static bool isUserCanBuyOrGiftItem(int iItemDescNum)
		{
			bool result = true;
			if (getItemDataFromItemDescNum(iItemDescNum, out var itemData))
			{
				if (!itemData.isPosition(eFuncItemPosition.eFuncItemPosition_CAPSULEMACHINE) && itemData.isFree)
				{
					Log.Error("TR and Cash and FarmPoint and GuildPoint and ContributionPoint price of the item({0}) is 0", iItemDescNum);
					result = false;
				}
				if (!itemData.m_bPurchasable)
				{
					Log.Error("Tried no ShowInShop item({0}) requested", iItemDescNum);
					result = false;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool getItemAttrValue(int iItemNum, eItemAttr attr, out float fValue)
		{
			fValue = 0f;
			if (!m_itemDescNumToItemData.TryGetValue(iItemNum, out var value) && value.m_mapAttr.TryGetValue((short)attr, out var value2))
			{
				fValue = value2;
				return true;
			}
			return false;
		}

		public static bool isOnOffItem(int iItemNum)
		{
			return m_onoffItemList.Contains(iItemNum);
		}

		public static bool isManCharacter(cpk_type iChar)
		{
			return m_manCharNums != null && m_manCharNums.Contains((int)iChar);
		}

		public static bool isWomanCharacter(cpk_type iChar)
		{
			return m_womanCharNums != null && m_womanCharNums.Contains((int)iChar);
		}
	}
}
