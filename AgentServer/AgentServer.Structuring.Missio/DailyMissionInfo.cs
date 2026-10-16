using System.Collections.Concurrent;

namespace AgentServer.Structuring.Mission
{
	public class DailyMissionInfo
	{
		public DailyMissionFinishedInfo FinishedInfo = new DailyMissionFinishedInfo();

		public ConcurrentDictionary<int, MissionInfo> MissionInfo = new ConcurrentDictionary<int, MissionInfo>();
	}
}
