namespace TRCommon
{
	public class CNetShuItemInfo : ItemKeyInfo
	{
		private int m_avataritemNum;

		private cpk_type m_avatarItemKind;

		public int getAvataritemNum => m_avataritemNum;

		public cpk_type getAvatarItemKind => m_avatarItemKind;

		public CNetShuItemInfo(int avataritemNum, cpk_type avatarItemKind, cpk_type c, cpk_type p, cpk_type k)
			: base(c, p, k)
		{
			m_avataritemNum = avataritemNum;
			m_avatarItemKind = avatarItemKind;
		}
	}
}
