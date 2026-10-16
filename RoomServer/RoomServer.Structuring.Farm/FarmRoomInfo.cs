namespace RoomServer.Structuring.Farm
{
	public class FarmRoomInfo
	{
		public bool IsMaster;

		public int FarmTypeNum;

		public int FarmSkyTypeNum;

		public int FarmWeatherTypeNum;

		public byte MaxPlayerNum;

		public string FarmName;

		public string MasterName;

		public long ExpireTime;

		public long CreateTime;

		public string Password;

		public int TotalCount;

		public int TodaysVisitorCount;

		public bool PremiumFarmUsing;

		public long PremiumFarmExpireDateTime;

		public int farmExp;

		public bool isPublic;

		public byte Type;

		public byte MaxUserLimit;

		public byte unk1;

		public bool isGuildFarm;
	}
}
