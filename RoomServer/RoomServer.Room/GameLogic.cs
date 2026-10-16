using System;
using System.Collections.Generic;
using System.Linq;
using RoomServer.Structuring;
using Serilog;

namespace RoomServer.Room
{
	public static class GameLogic
	{
		public static void getNormalMode_baseTREXP(Account User, NormalRoom room, byte rank, int realrank, out int baseTR, out int baseEXP)
		{
			baseTR = 0;
			baseEXP = 0;
			try
			{
				if (room.IsTeamPlay != 0)
				{
					if (room.RoomKindID == 81 || room.RoomKindID == 82 || room.RoomKindID == 83)
					{
						int team = (from p in room.PlayerList()
							orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
							select p).FirstOrDefault().Team;
						if (User.Team == team)
						{
							baseTR = RoomResultTable.getWinTeamGuildMatchGameMoney(room.PlayerCount());
							baseEXP = RoomResultTable.getWinTeamGuildMatchExp(room.PlayerCount());
						}
						else
						{
							baseTR = RoomResultTable.getLoseTeamGuildMatchGameMoney(room.PlayerCount());
							baseEXP = RoomResultTable.getLoseTeamGuildMatchExp(room.PlayerCount());
						}
					}
					else
					{
						int team2 = (from p in room.PlayerList()
							orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
							select p).FirstOrDefault().Team;
						if (User.Team == team2)
						{
							baseTR = RoomResultTable.getWinTeamGameMoney(room.PlayerCount());
							baseEXP = RoomResultTable.getWinTeamExp(room.PlayerCount());
						}
						else
						{
							baseTR = RoomResultTable.getLoseTeamGameMoney(room.PlayerCount());
							baseEXP = RoomResultTable.getLoseTeamExp(room.PlayerCount());
						}
					}
				}
				else if (User.Rank != 99)
				{
					baseTR = RoomResultTable.getGamePoint(realrank, room.PlayerCount());
					baseEXP = RoomResultTable.getExp(realrank, room.PlayerCount());
				}
				else
				{
					baseTR = RoomResultTable.getTimeOutGamePoint(room.PlayerCount());
					baseEXP = RoomResultTable.getTimeOutExp(room.PlayerCount());
				}
			}
			catch (Exception ex)
			{
				baseTR = 0;
				baseEXP = 0;
				Log.Error("[Test]getNormalModeReward Error:{0}", ex.ToString());
			}
		}

		public static void getRelayMode_baseTREXP(Account User, NormalRoom room, byte rank, int realrank, out int baseTR, out int baseEXP)
		{
			baseTR = 0;
			baseEXP = 0;
			try
			{
				int num = 1;
				IEnumerable<byte> enumerable = (from p in room.PlayerList()
					orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
					select p into s
					select s.RelayTeam).Distinct();
				int iRank = 0;
				foreach (byte i in enumerable)
				{
					if (i == User.RelayTeam)
					{
						iRank = ((!room.PlayerList().Any((Account x) => x.RelayTeam == i && x.Rank == 99)) ? num : 0);
						break;
					}
					num++;
				}
				baseTR = RoomResultTable.getRelayTeamResultGameMoney(iRank, enumerable.Count());
				baseTR = RoomResultTable.getRelayTeamResultExp(iRank, enumerable.Count());
			}
			catch (Exception ex)
			{
				baseTR = 0;
				baseEXP = 0;
				Log.Error("[Test]getRelayModeReward Error:{0}", ex.ToString());
			}
		}
	}
}
