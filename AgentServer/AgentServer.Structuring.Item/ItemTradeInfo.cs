using System.Collections.Generic;

namespace AgentServer.Structuring.Item
{
	public class ItemTradeInfo
	{
		public int m_ticket;

		public int m_result_count;

		public int m_min_unduplicated_item;

		public Dictionary<int, float> m_position2ratio = new Dictionary<int, float>();

		public Dictionary<int, float> m_levelratio = new Dictionary<int, float>();
	}
}
