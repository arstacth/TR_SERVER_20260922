using System.Collections.Generic;

namespace AgentServer.Structuring.Item
{
	public class CCubeOpenAck
	{
		private int m_cube;

		public eCUBE_TYPE cube_type;

		public List<ITEM_OPEN_INFO> items_open;

		public List<ITEM_UNOPEN_INFO> items_unopen;

		private eCUBE_OPEN_RESULT m_result;

		public int max_acceptable;

		public eCUBE_OPEN_RESULT open_result => m_result;

		public CCubeOpenAck(int c, eCUBE_TYPE t, eCUBE_OPEN_RESULT r, int a)
		{
			m_cube = c;
			cube_type = t;
			m_result = r;
			max_acceptable = a;
			items_open = new List<ITEM_OPEN_INFO>();
			items_unopen = new List<ITEM_UNOPEN_INFO>();
		}

		public void set_result(eCUBE_OPEN_RESULT r)
		{
			m_result = r;
		}
	}
}
