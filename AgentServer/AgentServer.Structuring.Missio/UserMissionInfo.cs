using System.Collections.Concurrent;

namespace AgentServer.Structuring.Mission
{
	public class UserMissionInfo
	{
		public ConcurrentDictionary<int, MissionInfo> MissionInfo = new ConcurrentDictionary<int, MissionInfo>();
	}
}
