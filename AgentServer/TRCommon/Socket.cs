using System.Collections.Generic;

namespace TRCommon
{
	public class Socket
	{
		public byte m_iSocketNum;

		public int m_iMountStoneNum;

		public Dictionary<short, float> m_attr = new Dictionary<short, float>();

		public Socket()
		{
			clear();
		}

		public Socket(byte iSocketNum, int iMountStoneNum, Dictionary<short, float> attr)
		{
			m_iSocketNum = iSocketNum;
			m_iMountStoneNum = iMountStoneNum;
			m_attr = attr;
		}

		public void clear()
		{
			m_iSocketNum = 0;
			m_iMountStoneNum = 0;
			m_attr.Clear();
		}

		public void removeStone()
		{
			m_iMountStoneNum = 0;
			m_attr.Clear();
		}
	}
}
