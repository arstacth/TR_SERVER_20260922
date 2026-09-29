using System;

namespace RoomServer.Structuring.GameReward
{
	public class GameRewardGroupInfo
	{
		public short GroupType;

		public int Argument;

		public int ChildGroupNum;

		public float SpecialRewardRate;

		public bool UsePeriod;

		public DateTime StartTime;

		public DateTime EndTime;

		public int StartHour;

		public int EndHour;
	}
}
