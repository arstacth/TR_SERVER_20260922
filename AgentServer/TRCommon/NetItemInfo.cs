namespace TRCommon
{
	public class NetItemInfo : ItemKeyInfo
	{
		public int m_iItemDescNum;

		public long m_expireTime;

		public long m_tGot;

		public int m_count;

		public int m_exp;

		public bool m_bHasExpireTime;

		public bool m_bUsing;

		public bool valid => m_iItemDescNum > 0;

		public bool isEquipmentItem
		{
			get
			{
				if (!NetCommonFunc.isWearItemPosition(m_position))
				{
					return NetCommonFunc.isTopBodyWearItemPosition(m_position);
				}
				return true;
			}
		}

		public NetItemInfo(int itemNum, cpk_type character, cpk_type position, cpk_type kind, bool bUsing, int count, long expiretime = 0L, long tGot = 0L)
		{
			m_exp = 0;
			m_iItemDescNum = itemNum;
			m_character = character;
			m_position = position;
			m_kind = kind;
			m_bUsing = bUsing;
			m_count = count;
			m_expireTime = expiretime;
			m_tGot = tGot;
			m_bHasExpireTime = 0 < expiretime;
		}

		public NetItemInfo()
		{
			m_iItemDescNum = 0;
			m_expireTime = 0L;
			m_tGot = 0L;
			m_count = 0;
			m_bHasExpireTime = false;
			m_exp = 0;
			m_bUsing = false;
		}

		public override void clear()
		{
			base.clear();
			m_iItemDescNum = 0;
			m_expireTime = 0L;
			m_tGot = 0L;
			m_count = 0;
			m_bHasExpireTime = false;
			m_exp = 0;
			m_bUsing = false;
		}

		public bool find(int item)
		{
			return m_iItemDescNum == item;
		}
	}
}
