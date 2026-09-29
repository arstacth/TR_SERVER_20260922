using System.Collections.Generic;

namespace AgentServer.Structuring.HotTime
{
	public class HotTimeInfo
	{
		public int HotTimeType;

		public int HotTimeImportant;

		public long StartTime;

		public long FinishTime;

		public long RequiredTime;

		public int LimitedUserNum;

		public int RewardKind;

		public List<HotTimeRewardInfo> RewardInfos = new List<HotTimeRewardInfo>();
	}
}
