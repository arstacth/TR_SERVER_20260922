using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoomServer.Structuring.Shu
{
	public class DBShuInfo
	{
		public List<long> characterItemID = new List<long>();

		public ShuEggInfo eggitem = new ShuEggInfo();

		public ConcurrentDictionary<long, List<ShuItemInfo>> shuitems = new ConcurrentDictionary<long, List<ShuItemInfo>>();

		public ConcurrentDictionary<long, ShuCharInfo> shuchars = new ConcurrentDictionary<long, ShuCharInfo>();

		public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();

		public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();
	}
}
