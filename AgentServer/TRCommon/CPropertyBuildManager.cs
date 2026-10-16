using System.Collections.Generic;

namespace TRCommon
{
	public class CPropertyBuildManager
	{
		private CItemAttr m_resultAttr;

		public CPropertyBuildManager()
		{
			m_resultAttr = new CItemAttr();
		}

		public void clear()
		{
			m_resultAttr.clear();
		}

		public void addProperty(CPropertyCheckSource source)
		{
			CItemAttr getAttrs = source.getAttrs;
			if (!_checkAvatar(source.getAvatarType, getAttrs))
			{
				return;
			}
			foreach (KeyValuePair<short, float> item in getAttrs.m_attr)
			{
				short key = item.Key;
				float value = item.Value;
				m_resultAttr.m_attr[key] += value;
			}
		}

		public void addProperty(List<CPropertyCheckSource> vecPropertyCheckSource)
		{
			foreach (CPropertyCheckSource item in vecPropertyCheckSource)
			{
				addProperty(item);
			}
		}

		public CItemAttr getResultAttr()
		{
			return m_resultAttr;
		}

		public bool _checkAvatar(int avatarType, CItemAttr attrs)
		{
			int num = (int)attrs.m_attr[5001];
			if (avatarType != 1 || num != 2)
			{
				if (avatarType == 2)
				{
					return num >= 2;
				}
				return true;
			}
			return false;
		}
	}
}
