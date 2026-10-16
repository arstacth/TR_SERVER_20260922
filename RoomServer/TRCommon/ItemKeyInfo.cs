namespace TRCommon
{
	public class ItemKeyInfo
	{
		public cpk_type m_character;

		public cpk_type m_position;

		public cpk_type m_kind;

		public bool isCharacter
		{
			get
			{
				if ((int)m_character != 0 && (int)m_position == 0)
				{
					return (int)m_kind == 0;
				}
				return false;
			}
		}

		public ItemKeyInfo()
		{
			m_character = 0;
			m_position = 0;
			m_kind = 0;
		}

		public ItemKeyInfo(cpk_type c, cpk_type p, cpk_type k)
		{
			m_character = c;
			m_position = p;
			m_kind = k;
		}

		public virtual void clear()
		{
			m_character = 0;
			m_position = 0;
			m_kind = 0;
		}

		public void setItemKey(cpk_type nCharacter, cpk_type nPart, cpk_type nKind)
		{
			m_character = nCharacter;
			m_position = nPart;
			m_kind = nKind;
		}

		public static bool operator <(ItemKeyInfo cKey, ItemKeyInfo cKey2)
		{
			if ((int)cKey.m_character < (int)cKey2.m_character)
			{
				return true;
			}
			if ((int)cKey.m_character == (int)cKey2.m_character)
			{
				if ((int)cKey.m_position < (int)cKey2.m_position)
				{
					return true;
				}
				if ((int)cKey.m_position == (int)cKey2.m_position && (int)cKey.m_kind < (int)cKey2.m_kind)
				{
					return true;
				}
			}
			return false;
		}

		public static bool operator >(ItemKeyInfo cKey, ItemKeyInfo cKey2)
		{
			if ((int)cKey.m_character > (int)cKey2.m_character)
			{
				return true;
			}
			if ((int)cKey.m_character == (int)cKey2.m_character)
			{
				if ((int)cKey.m_position > (int)cKey2.m_position)
				{
					return true;
				}
				if ((int)cKey.m_position == (int)cKey2.m_position && (int)cKey.m_kind > (int)cKey2.m_kind)
				{
					return true;
				}
			}
			return false;
		}
	}
}
