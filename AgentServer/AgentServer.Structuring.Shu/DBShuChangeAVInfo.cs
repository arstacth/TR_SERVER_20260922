using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AgentServer.Structuring.Shu
{
	public class DBShuChangeAVInfo
	{
		public List<ShuAvatarState> AvatarState = new List<ShuAvatarState>();

		public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();
	}
}
