using System.Collections.Generic;

namespace AgentServer.Structuring.Mission
{
	public class MissionInfo
	{
		public int MissionNum;

		public int MissionKind;

		public int challengeState;

		public long challengeExpireTime;

		public int collectionType;

		public Dictionary<int, MissionConditionInfo> ConditionInfo = new Dictionary<int, MissionConditionInfo>();
	}
}
