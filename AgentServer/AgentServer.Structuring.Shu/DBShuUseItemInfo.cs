using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AgentServer.Structuring.Shu
{
	public class DBShuUseItemInfo
	{
		public List<ShuItemInfo> ItemInfos = new List<ShuItemInfo>();

		public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();

		public int remainMP;

		public ConcurrentDictionary<long, ShuCharInfo> shuchars = new ConcurrentDictionary<long, ShuCharInfo>();

		public List<long> characterItemID = new List<long>();

		public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();

		public int beforeLevel;

		public int afterLevel;
	}
}
