using System.Collections.Generic;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Guild;

namespace RoomServer.Structuring
{
	public class DropList
	{
		public int UserNum;

		public string NickName;

		public int FreePassType;

		public long TotalEXP;

		public float RaceDistance;

		public int ServerLapTime;

		public int LapTime;

		public byte Pos;

		public byte Team;

		public byte RelayTeam;

		public byte RelayTeamPos;

		public ushort BounsTR;

		public ushort BounsEXP;

		public int TR;

		public int EXP;

		public byte Rank;

		public bool isLevelUP;

		public List<int> CardID = new List<int>();

		public List<GameRewardResult> RewardItemID = new List<GameRewardResult>();

		public int TotalDamage;

		public int MaxDamage;

		public GuildInfo GuildInfo;

		public int MiniGamePoint;

		public int MiniGameStarPoint;

		public CGameResultDesc GameResultDesc = new CGameResultDesc();
	}
}
