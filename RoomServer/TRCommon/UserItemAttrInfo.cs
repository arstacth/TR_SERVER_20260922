using System.Collections.Generic;

namespace TRCommon
{
	public class UserItemAttrInfo
	{
		public int m_iItemDescNum;

		public Dictionary<short, float> m_ItemAttr;

		public UserItemAttrInfo()
		{
			m_iItemDescNum = 0;
			m_ItemAttr = new Dictionary<short, float>();
		}

		public static UserItemAttrInfo operator +(UserItemAttrInfo a, UserItemAttrInfo b)
		{
			if (a.m_iItemDescNum == 0)
			{
				a.m_iItemDescNum = b.m_iItemDescNum;
			}
			if (a.m_iItemDescNum == b.m_iItemDescNum)
			{
				foreach (KeyValuePair<short, float> item in b.m_ItemAttr)
				{
					short key = item.Key;
					float value = item.Value;
					a.m_ItemAttr[key] += value;
				}
				return a;
			}
			return a;
		}
	}
}
