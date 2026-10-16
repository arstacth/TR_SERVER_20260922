namespace TRCommon
{
	public class CItemInfo_RealTime
	{
		protected int m_itemnum;

		protected bool mb_showshop;

		protected bool mb_purchasable;

		protected int m_etc;

		protected int m_schedule_subkey;

		public int item_num => m_itemnum;

		public bool is_showshop => mb_showshop;

		public bool is_purchasable => mb_purchasable;

		public int etc => m_etc;

		public int schedule_subkey => m_schedule_subkey;

		public CItemInfo_RealTime()
		{
		}

		public CItemInfo_RealTime(int itemnu, bool is_showshop, bool is_purchasable, int etc, int schedule_key)
		{
			m_itemnum = itemnu;
			mb_showshop = is_showshop;
			mb_purchasable = is_purchasable;
			m_etc = etc;
			m_schedule_subkey = schedule_key;
		}

		public bool find_by_id(int itemnum)
		{
			return m_itemnum == itemnum;
		}

		public bool find_by_schedule_key(int key)
		{
			return m_schedule_subkey == key;
		}
	}
}
