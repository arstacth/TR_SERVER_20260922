using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoomServer.Structuring.Shu
{
	public class DBShuActionInfo
	{
		public int remainMP;

		public List<ShuActionResultInfo> ActionResult = new List<ShuActionResultInfo>();

		public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();

		public int beforeLevel;

		public int afterLevel;
	}
}
