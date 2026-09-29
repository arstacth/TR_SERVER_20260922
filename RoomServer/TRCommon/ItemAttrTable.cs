using System.Collections.Generic;

namespace TRCommon
{
	public class ItemAttrTable
	{
		public static Dictionary<int, CItemAttr> m_itemAttributes;

		public static CItemAttr m_emptyItemAttr = new CItemAttr();

		public static void onRecvItemAttr(Dictionary<int, CItemAttr> data)
		{
			m_itemAttributes = data;
		}

		public static bool getItemAttrFromItemDescNum(int iItemDescNum, out CItemAttr it)
		{
			return m_itemAttributes.TryGetValue(iItemDescNum, out it);
		}

		public static float getItemAttrFromItemDescNum(int iItemDescNum, eItemAttr attr)
		{
			if (getItemAttrFromItemDescNum(iItemDescNum, out var it))
			{
				return it.m_attr[(short)attr];
			}
			return 0f;
		}
	}
}
