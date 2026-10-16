using System.Collections.Generic;

namespace TRCommon
{
	public class ItemDataFromDatabase
	{
		public int m_iItemDescNum;

		public int m_iBillingItemID;

		public int m_iType;

		public int m_iCharacter;

		public int m_iPosition;

		public int m_iItemKind;

		public int m_iTRPrice;

		public int m_iCashPrice;

		public int m_iFarmPointPrice;

		public int m_iGuildPointPrice;

		public int m_iContributionPointPrice;

		public int m_iGotRateKind;

		public int m_iOnOffType;

		public eCoupleItemType m_eCoupleItemType;

		public bool m_bShowInShop;

		public bool m_bPurchasable;

		public int m_iSupplyItemDescNum;

		public int m_iCategory;

		public string m_strItemName;

		public Dictionary<short, float> m_mapAttr;

		public int m_iEtc;

		public bool m_bRefundable;

		public bool m_bCanStack;

		public List<int> m_packageItemNums;

		public bool m_bNotDeleteWhenExpired;

		public bool m_bHasExpireTime;

		public bool isFree
		{
			get
			{
				if (0 >= m_iTRPrice && 0 >= m_iCashPrice && 0 >= m_iFarmPointPrice && 0 >= m_iGuildPointPrice)
				{
					return 0 >= m_iContributionPointPrice;
				}
				return false;
			}
		}

		public bool isRefundable => m_bRefundable;

		public bool isPet => NetCommonFunc.isPetPosition(m_iPosition);

		public bool isEquipItem
		{
			get
			{
				if (!NetCommonFunc.isWearItemPosition(m_iPosition))
				{
					return NetCommonFunc.isTopBodyWearItemPosition(m_iPosition);
				}
				return true;
			}
		}

		public bool isFreePassSetItem
		{
			get
			{
				if (!isPosition(eFuncItemPosition.eFuncItemPosition_FREE_PASS_SET_ITEM))
				{
					return isPosition(eFuncItemPosition.eFuncItemPosition_FREE_PASS_PREMIUM_SET_ITEM);
				}
				return true;
			}
		}

		public bool isRentalItem
		{
			get
			{
				if (!isEtc(eItemEtc.eItemEtc_CLOTHES_RENTAL))
				{
					return isEtc(eItemEtc.eItemEtc_CLOTHES_RENTAL_PREMIUM);
				}
				return true;
			}
		}

		public bool isPeriodicItem
		{
			get
			{
				if (m_bHasExpireTime)
				{
					return !m_bNotDeleteWhenExpired;
				}
				return false;
			}
		}

		public bool isDurabilityItem
		{
			get
			{
				if (m_bHasExpireTime)
				{
					return m_bNotDeleteWhenExpired;
				}
				return false;
			}
		}

		public ItemDataFromDatabase()
		{
			m_mapAttr = new Dictionary<short, float>();
			m_packageItemNums = new List<int>();
		}

		public bool isEtc(eItemEtc eEtc)
		{
			return ((uint)m_iEtc & (uint)eEtc) == (uint)eEtc;
		}

		public bool isPosition(eFuncItemPosition pos)
		{
			return m_iPosition == (int)pos;
		}

		public int getSupplyItemNum()
		{
			if (1 == m_iType)
			{
				return 0;
			}
			if (3 == m_iType)
			{
				return m_iSupplyItemDescNum;
			}
			return m_iItemDescNum;
		}

		public int getRealItemNum()
		{
			switch (m_iType)
			{
			case 1:
			case 2:
			case 4:
				return m_iItemDescNum;
			case 3:
				return m_iSupplyItemDescNum;
			default:
				return m_iItemDescNum;
			}
		}
	}
}
