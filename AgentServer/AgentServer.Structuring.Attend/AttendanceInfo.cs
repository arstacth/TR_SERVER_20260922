namespace AgentServer.Structuring.Attendance
{
	public class AttendanceInfo
	{
		public int AttendanceKey;

		public int AttendanceType;

		public int AttendanceRewardGroupKey;

		public long Start;

		public long EndS;

		public long EndU;

		public int AttendancePoint;

		public UserAttendanceInfo userAttendanceInfos = new UserAttendanceInfo();
	}
}
