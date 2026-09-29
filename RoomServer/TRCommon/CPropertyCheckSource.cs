namespace TRCommon
{
	public class CPropertyCheckSource
	{
		private CItemAttr m_attrs;

		private int m_iAvatarType;

		private cpk_type m_relatedItemPosition;

		public int getAvatarType => m_iAvatarType;

		public cpk_type getPosition => m_relatedItemPosition;

		public CItemAttr getAttrs => m_attrs;

		public CPropertyCheckSource(CItemAttr attr, cpk_type position, int avatarType = 0)
		{
			m_attrs = attr;
			m_iAvatarType = avatarType;
			m_relatedItemPosition = position;
		}

		public void setCheckSource(CItemAttr attr, int avatarType, cpk_type position)
		{
			m_attrs = attr;
			m_iAvatarType = avatarType;
			m_relatedItemPosition = position;
		}
	}
}
