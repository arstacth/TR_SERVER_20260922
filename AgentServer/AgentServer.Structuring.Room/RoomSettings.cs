using AgentServer.Structuring.Farm;

namespace AgentServer.Structuring.Room
{
	public class RoomSettings
	{
		public string Name;

		public string Password;

		public int IsTeamPlay;

		public int ItemType;

		public bool IsStepOn;

		public int MapNum = 1;

		public int RoomKindID;

		public RoomKindInfo roomkindinfo;

		public int FarmIndex;

		public FarmRoomInfo FarmRoomInfo;
	}
}
