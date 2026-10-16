namespace TRCommon
{
	public class BuyingItemInfo
	{
		public int m_iItemDescNum;

		public long m_iFarmItemID;

		public eShopPriceType m_ePriceType;

		public bool m_bStandbyPurchaseDicision;

		public bool m_bBuyPurchasable;

		public int m_iSellItemNum;

		public bool isStandbyPurchaseDicision => m_bStandbyPurchaseDicision;

		public BuyingItemInfo()
		{
			m_iItemDescNum = 0;
			m_iFarmItemID = -1L;
			m_ePriceType = eShopPriceType.eShopPriceType_UNKNOWN;
			m_bStandbyPurchaseDicision = false;
			m_bBuyPurchasable = true;
			m_iSellItemNum = 0;
		}

		public BuyingItemInfo(int iItemDescNum)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = -1L;
			m_ePriceType = eShopPriceType.eShopPriceType_UNKNOWN;
			m_bStandbyPurchaseDicision = false;
			m_bBuyPurchasable = true;
		}

		public BuyingItemInfo(int iItemDescNum, long iFarmItemID)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = iFarmItemID;
			m_ePriceType = eShopPriceType.eShopPriceType_UNKNOWN;
			m_bStandbyPurchaseDicision = false;
			m_bBuyPurchasable = true;
		}

		public BuyingItemInfo(int iItemDescNum, long iFarmItemID, eShopPriceType paymentType)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = iFarmItemID;
			m_ePriceType = paymentType;
			m_bStandbyPurchaseDicision = false;
			m_bBuyPurchasable = true;
		}

		public BuyingItemInfo(int iItemDescNum, long iFarmItemID, eShopPriceType paymentType, bool bStandbyPurchaseDicision)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = iFarmItemID;
			m_ePriceType = paymentType;
			m_bStandbyPurchaseDicision = bStandbyPurchaseDicision;
			m_bBuyPurchasable = true;
		}

		public BuyingItemInfo(int iItemDescNum, long iFarmItemID, eShopPriceType paymentType, bool bStandbyPurchaseDicision, bool bBuyPurchasable)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = iFarmItemID;
			m_ePriceType = paymentType;
			m_bStandbyPurchaseDicision = bStandbyPurchaseDicision;
			m_bBuyPurchasable = bBuyPurchasable;
		}

		public BuyingItemInfo(int iItemDescNum, long iFarmItemID, eShopPriceType paymentType, bool bStandbyPurchaseDicision, bool bBuyPurchasable, int iSellItemNum)
		{
			m_iItemDescNum = iItemDescNum;
			m_iFarmItemID = iFarmItemID;
			m_ePriceType = paymentType;
			m_bStandbyPurchaseDicision = bStandbyPurchaseDicision;
			m_bBuyPurchasable = bBuyPurchasable;
			m_iSellItemNum = iSellItemNum;
		}
	}
}
