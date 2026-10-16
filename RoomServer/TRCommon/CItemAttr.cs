using System;

namespace TRCommon
{
	[Serializable]
	public class CItemAttr
	{
		public CItemAttrNormal m_attr;

		public CItemAttr()
		{
			m_attr = new CItemAttrNormal();
			clear();
		}

		public void clear()
		{
			m_attr.clear();
		}

		public static CItemAttr operator +(CItemAttr a, CItemAttr b)
		{
			a.m_attr += b.m_attr;
			return a;
		}

		public static CItemAttr operator -(CItemAttr a, CItemAttr b)
		{
			a.m_attr -= b.m_attr;
			return a;
		}
	}
}
