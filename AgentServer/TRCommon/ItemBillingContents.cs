namespace TRCommon
{
	public class ItemBillingContents
	{
		public int m_iItemDescNum;

		public int m_iBillItemID;

		public int m_iGameMoneyPrice;

		public int m_iCashPrice;

		public int m_iFarmPointPrice;

		public int m_iGuildPointPrice;

		public int m_iContributionPointPrice;

		public string m_strItemName;

		public eShopPriceType m_paymentType;

		public int m_iDiscountRate;

		public int m_iDiscountPrice;

		public int m_iSellItemNum;

		public int m_iMileagePrice;

		public bool isFree
		{
			get
			{
				if (0 >= m_iGameMoneyPrice && 0 >= m_iCashPrice && 0 >= m_iFarmPointPrice && 0 >= m_iGuildPointPrice)
				{
					return 0 >= m_iContributionPointPrice;
				}
				return false;
			}
		}

		public ItemBillingContents()
		{
			m_iDiscountRate = 0;
			m_iDiscountPrice = 0;
		}

		public ItemBillingContents(int itemdescnum, int iBillingItemID, int iTRPrice, int iCashPrice, int iFarmPointPrice, int iGuildPointPrice, int iContributionPointPrice, string strItemName, eShopPriceType paymentType)
		{
			m_iItemDescNum = itemdescnum;
			m_iBillItemID = iBillingItemID;
			m_iGameMoneyPrice = iTRPrice;
			m_iCashPrice = iCashPrice;
			m_iFarmPointPrice = iFarmPointPrice;
			m_iGuildPointPrice = iGuildPointPrice;
			m_iContributionPointPrice = iContributionPointPrice;
			m_paymentType = paymentType;
			m_strItemName = strItemName;
			m_iDiscountRate = 0;
			m_iDiscountPrice = 0;
			m_iSellItemNum = 0;
			m_iMileagePrice = 0;
		}

		public bool isUseCouponForFree()
		{
			if (100 == m_iDiscountRate)
			{
				return true;
			}
			return m_paymentType switch
			{
				eShopPriceType.eShopPriceType_GAMEMONEY => m_iDiscountPrice == m_iGameMoneyPrice, 
				eShopPriceType.eShopPriceType_CASH => m_iDiscountPrice == m_iCashPrice, 
				eShopPriceType.eShopPriceType_FARMPOINT => m_iDiscountPrice == m_iFarmPointPrice, 
				eShopPriceType.eShopPriceType_GUILDPOINT => m_iDiscountPrice == m_iGuildPointPrice, 
				eShopPriceType.eShopPriceType_CONTRIBUTIONPOINT => m_iDiscountPrice == m_iContributionPointPrice, 
				_ => false, 
			};
		}

		public int getPrice()
		{
			return m_paymentType switch
			{
				eShopPriceType.eShopPriceType_GAMEMONEY => m_iGameMoneyPrice, 
				eShopPriceType.eShopPriceType_CASH => m_iCashPrice, 
				eShopPriceType.eShopPriceType_FARMPOINT => m_iFarmPointPrice, 
				eShopPriceType.eShopPriceType_GUILDPOINT => m_iGuildPointPrice, 
				eShopPriceType.eShopPriceType_CONTRIBUTIONPOINT => m_iContributionPointPrice, 
				eShopPriceType.eShopPriceType_MILEAGE => m_iMileagePrice, 
				_ => 0, 
			};
		}

		public void setPrice(int iPrice)
		{
			switch (m_paymentType)
			{
			case eShopPriceType.eShopPriceType_GAMEMONEY:
				m_iGameMoneyPrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_CASH:
				m_iCashPrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_FARMPOINT:
				m_iFarmPointPrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_GUILDPOINT:
				m_iGuildPointPrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_CONTRIBUTIONPOINT:
				m_iContributionPointPrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_MILEAGE:
				m_iMileagePrice = iPrice;
				break;
			case eShopPriceType.eShopPriceType_ITEM:
				break;
			}
		}
	}
}
