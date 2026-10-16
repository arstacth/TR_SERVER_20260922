using System.Collections.Generic;

namespace AgentServer.Structuring.Mission
{
	public class DailyMissionFinishedInfo
	{
		public Dictionary<int, int> MissionFinishedInfo = new Dictionary<int, int>();

		public DailyMissionFinishedInfo()
		{
			MissionFinishedInfo.Add(3, 0);
			MissionFinishedInfo.Add(4, 0);
			MissionFinishedInfo.Add(5, 0);
		}
	}
}
