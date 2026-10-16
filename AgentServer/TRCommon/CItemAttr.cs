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
			if (a == null)
			{
				return b ?? new CItemAttr();
			}
			if (b == null || b.m_attr == null)
			{
				return a;
			}
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
