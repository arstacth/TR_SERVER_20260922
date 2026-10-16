using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.Room;
using RoomServer.Holders;
using RoomServer.Packet.Send;
using RoomServer.Room;
using RoomServer.Structuring;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Opcode;
using Serilog;

namespace RoomServer.Packet
{
	public class GameRoomEvent
	{
		public static async void Execute_GameEnd(NormalRoom room, long EndTime, byte last)
		{
			if (room.RoomStatus != 2)
			{
				return;
			}
			room.RoomStatus = 3;
			while (((Utility.CurrentTimeMilliseconds() < EndTime && !room.RunlympicMode) || room.RunlympicMode) && !room.Players.Values.Where((Account p) => p.Attribute != 3).All((Account p) => p.GameEndType > 0))
			{
				await Task.Delay(1000);
			}
			try
			{
				if (room.PlayerList().Exists((Account p) => p.GameEndType == 0 && p.Attribute != 3) && !room.RunlympicMode)
				{
					foreach (Account item in from p in room.PlayerList()
						where p.GameEndType == 0 && p.Attribute != 3
						select p into o
						orderby o.RaceDistance descending
						select o)
					{
						item.LapTime = room.GetCurrentTime() + 300000;
						item.ServerLapTime = item.LapTime;
						item.Rank = 99;
						item.GameEndType = 3;
					}
				}
				if (room.BonusStageLevel > 0)
				{
					int serverLapTime = (from o in room.PlayerList()
						orderby o.ServerLapTime
						select o).FirstOrDefault().ServerLapTime;
					foreach (Account item2 in from p in room.PlayerList()
						where p.Attribute != 3
						select p into o
						orderby o.BonusStagePoint descending
						select o)
					{
						item2.Rank = room.Rank++;
						int num3 = (item2.ServerLapTime = (item2.LapTime = serverLapTime + item2.Rank));
						if (item2.GameEndType == 3 || item2.GameEndType == 4)
						{
							item2.LapTime = room.GetCurrentTime() + 220000;
							item2.ServerLapTime = item2.LapTime;
							item2.Rank = 99;
						}
					}
				}
				else if (room.RunlympicMode)
				{
					foreach (Account item3 in from p in room.PlayerList()
						where p.Attribute != 3
						select p into o
						orderby o.RunlympicPoint descending
						select o)
					{
						item3.Rank = room.Rank++;
						if (item3.GameEndType != 1 || item3.GameEndType != 2)
						{
							item3.LapTime = room.GetCurrentTime() + 220000;
							item3.ServerLapTime = item3.LapTime;
						}
					}
				}
				else if (room.RuleType == 16384)
				{
					if (room.PlayerList().Exists((Account e) => e.GameEndType == 1))
					{
						int humanTeam = room.HumanTeam;
						foreach (Account item4 in from w in room.PlayerList()
							where w.Team != humanTeam
							select w)
						{
							item4.Rank = 102;
						}
						foreach (Account item5 in from w in room.PlayerList()
							where w.Team == humanTeam
							select w)
						{
							item5.Rank = 101;
						}
						foreach (Account item6 in from w in room.PlayerList()
							where w.GameEndType == 1
							select w into o
							orderby o.ServerLapTime, o.RaceDistance descending
							select o)
						{
							item6.Rank = room.Rank++;
						}
					}
					else
					{
						foreach (Account item7 in room.PlayerList())
						{
							if (item7.GameOver)
							{
								item7.Rank = 102;
							}
							else
							{
								item7.Rank = 101;
							}
						}
					}
				}
				else if (room.GameMode != 3 && room.RuleType != 256 && !room.RunlympicMode)
				{
					if (room.RuleType != 2 && room.RuleType != 4)
					{
						_ = room.RuleType == 8;
					}
					foreach (Account item8 in from p in room.PlayerList()
						where p.Attribute != 3
						select p into o
						orderby o.ServerLapTime, o.RaceDistance descending
						select o)
					{
						if (item8.GameEndType == 1)
						{
							item8.Rank = room.Rank++;
						}
						if (item8.GameEndType == 2)
						{
							item8.Rank = room.Rank++;
						}
						else if (item8.GameEndType == 3)
						{
							item8.LapTime = room.GetCurrentTime() + 220000;
							item8.ServerLapTime = item8.LapTime;
							item8.Rank = 99;
						}
						else if (item8.GameEndType == 4)
						{
							item8.LapTime = room.GetCurrentTime() + 240000;
							item8.ServerLapTime = item8.LapTime;
							if (room.RuleType == 4)
							{
								item8.Rank = 99;
							}
							else
							{
								item8.Rank = 98;
							}
						}
						else if (item8.GameEndType == 5)
						{
							item8.Rank = room.Rank++;
						}
					}
				}
				else if (room.RuleType == 256)
				{
					foreach (Account item9 in from p in room.PlayerList()
						where p.Attribute != 3
						select p into o
						orderby o.TotalDamage descending
						select o)
					{
						if (item9.GameEndType == 1)
						{
							item9.Rank = room.Rank++;
						}
						if (item9.GameEndType == 2)
						{
							item9.Rank = room.Rank++;
						}
						else if (item9.GameEndType == 3)
						{
							item9.LapTime = room.GetCurrentTime() + 220000;
							item9.ServerLapTime = item9.LapTime;
							item9.Rank = 99;
						}
						else if (item9.GameEndType == 4)
						{
							item9.LapTime = room.GetCurrentTime() + 240000;
							item9.ServerLapTime = item9.LapTime;
							if (room.RuleType == 4)
							{
								item9.Rank = 99;
							}
							else
							{
								item9.Rank = 98;
							}
						}
						else if (item9.GameEndType == 5)
						{
							item9.Rank = room.Rank++;
						}
					}
				}
				else
				{
					foreach (Account player in from w in room.PlayerList()
						where (w.RelayTeamPos == 5 || w.RelayTeamPos == 8 || w.RelayTeamPos == 11 || w.RelayTeamPos == 14 || w.RelayTeamPos == 17 || w.RelayTeamPos == 20) && w.Attribute != 3
						select w into o
						orderby o.ServerLapTime, o.RaceDistance descending
						select o)
					{
						if (player.GameEndType == 1)
						{
							player.Rank = room.Rank++;
							int num4 = 1;
							foreach (Account item10 in from w in room.PlayerList()
								where w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum && w.Attribute != 3
								select w)
							{
								item10.LapTime = player.LapTime + num4;
								item10.ServerLapTime = player.ServerLapTime + num4;
								item10.Rank = room.Rank++;
								num4++;
							}
						}
						if (player.GameEndType == 2)
						{
							player.Rank = room.Rank++;
							int num5 = 1;
							foreach (Account item11 in from w in room.PlayerList()
								where w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum && w.Attribute != 3
								select w)
							{
								item11.LapTime = player.LapTime + num5;
								item11.ServerLapTime = player.ServerLapTime + num5;
								item11.Rank = room.Rank++;
								num5++;
							}
						}
						else if (player.GameEndType == 3)
						{
							player.LapTime = room.GetCurrentTime() + 220000;
							player.ServerLapTime = player.LapTime;
							player.Rank = 99;
							foreach (Account item12 in from w in room.PlayerList()
								where w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum && w.Attribute != 3
								select w)
							{
								item12.LapTime = player.LapTime;
								item12.ServerLapTime = player.ServerLapTime;
								item12.Rank = player.Rank;
							}
						}
						else if (player.GameEndType == 4)
						{
							player.LapTime = room.GetCurrentTime() + 240000;
							player.ServerLapTime = player.LapTime;
							if (room.RuleType == 4)
							{
								player.Rank = 99;
							}
							else
							{
								player.Rank = 98;
							}
							foreach (Account item13 in from w in room.PlayerList()
								where w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum && w.Attribute != 3
								select w)
							{
								item13.LapTime = player.LapTime;
								item13.ServerLapTime = player.ServerLapTime;
								item13.Rank = player.Rank;
							}
						}
						else
						{
							if (player.GameEndType != 5)
							{
								continue;
							}
							player.Rank = room.Rank++;
							int num6 = 1;
							foreach (Account item14 in from w in room.PlayerList()
								where w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum && w.Attribute != 3
								select w)
							{
								item14.LapTime = player.LapTime + num6;
								item14.ServerLapTime = player.ServerLapTime + num6;
								item14.Rank = room.Rank++;
								num6++;
							}
						}
					}
				}
				float rankmultiply = 1f;
				if (room.GameMode == 5)
				{
					CorunModeResult(room, out rankmultiply);
				}
				int delay = 2000 / room.PlayerList().Count;
				byte realrank = 1;
				foreach (Account item15 in from p in room.PlayerList()
					where p.Attribute != 3
					orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
					select p)
				{
					if (room.BonusStageLevel == 0)
					{
						Calc_DropItem(item15, room, item15.Rank, (int)realrank++, rankmultiply);
					}
					else
					{
						Calc_BonusStageDropItem(item15, room, item15.Rank);
					}
					await Task.Delay(delay);
				}
				room.CouplePointUpdate();
				if ((room.RoomKindID == 81 || room.RoomKindID == 82 || room.RoomKindID == 83) && !room.PlayerList().All((Account a) => a.Rank >= 98))
				{
					Account winplayer = (from p in room.PlayerList()
						orderby p.Rank, p.RaceDistance descending
						select p).FirstOrDefault();
					Account account = room.PlayerList().FirstOrDefault((Account f) => f.GuildNum != winplayer.GuildNum);
					if (winplayer != null && account != null)
					{
						guildMatchRecord(winplayer.GuildNum, winplayer.GuildInfo.guildName, 0, 0, account.GuildNum, account.GuildInfo.guildName, 0, 0);
					}
					else
					{
						Log.Error("winplayer is null:{0}, loseplayer is null:{1}", winplayer == null, account == null);
					}
				}
				await Task.Delay(1000);
				if (room.DropItem.Count < 1)
				{
					return;
				}
				room.Result = GenResult(room, last);
				room.RegisterItem(-1, -1L, 2, 273281036, ispublic: true);
				room.GameRoomResult();
				await Task.Delay(1000);
				List<Account> playerlist = room.PlayerList();
				bool flag = room.GameMode == 3;
				foreach (Account item16 in playerlist)
				{
					item16.IsReady = false;
					if (flag)
					{
						int num7 = room.Players.Values.Count((Account p) => p.RelayTeamPos == 1);
						int num8 = room.Players.Values.Count((Account p) => p.RelayTeamPos == 2);
						byte relayteampos = (byte)((num7 <= num8) ? 1u : 2u);
						item16.SelectRelayTeam(relayteampos);
					}
				}
				foreach (Account levelUPPlayer in room.LevelUPPlayerList)
				{
					levelUPPlayer.SendAsync(new UserLevelUPEXPInfo(1, levelUPPlayer.Level, levelUPPlayer.Exp, last));
				}
				foreach (Account item17 in playerlist)
				{
					item17.SendAsync(new GameRoom_GameUpdateEXP_New(room.RoomKindID, item17, last));
				}
				foreach (Account item18 in playerlist)
				{
					item18.SendAsync(new GameRoom_GameResult2(item18, room.Result));
					item18.SendAsync(new GameRoom_Result(room.RoomKindID, last));
					foreach (Account item19 in room.Players.Values.ToList())
					{
						item18.SendAsync(new GameRoom_RoomPosReady(item19.RoomPos, item19.IsReady, last));
						if (flag)
						{
							item18.SendAsync(new GameRoom_RoomPosRelayTeam(item19, last));
						}
					}
					item18.SendAsync(new GameRoom_GoodsInfo(room, last));
				}
				if (room.LevelUPPlayerList.Count > 0)
				{
					foreach (Account item20 in playerlist)
					{
						foreach (Account levelUPPlayer2 in room.LevelUPPlayerList)
						{
							item20.SendAsync(new GameRoom_LevelUP(1, levelUPPlayer2.Exp, levelUPPlayer2.RoomPos, last));
						}
					}
				}
				foreach (Account item21 in playerlist)
				{
					item21.SendAsync(new GameRoom_UpdateIndividualGameRecord(item21, last));
				}
				foreach (Account item22 in playerlist.Where((Account p) => p.Attribute != 3))
				{
					if (room.DropItem.ContainsKey(item22.UserNum))
					{
						List<GameRewardResult> rewardItemID = room.DropItem[item22.UserNum].RewardItemID;
						if (rewardItemID.Count > 0)
						{
							item22.SendAsync(new GameRoom_RewardResult(rewardItemID, last));
						}
					}
				}
				foreach (Account item23 in playerlist.FindAll((Account p) => p.NeedVertificated))
				{
					item23.SendAsync(new ClientCheckAutoBanACK(7, last));
					item23.isDisconnected = true;
					VertificationBan(item23);
				}
				foreach (Account item24 in playerlist)
				{
					item24.MatchTime++;
					if (item24.MatchTime % 10 == 0)
					{
						item24.VertificationCode = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(0, 100);
						item24.NeedVertificated = true;
					}
				}
				await Task.Delay(6000);
				foreach (Account item25 in playerlist)
				{
					item25.SendAsync(new MoveToGameRoom(last));
					if (item25.NeedVertificated)
					{
						item25.SendAsync(new GameRoom_GetVertificationInfo(item25, last));
					}
				}
				room.GameEndSetNewRoomMaster();
				room.StartAutoChangeRoomMaster();
				room.GameEndReset();
			}
			catch (Exception ex)
			{
				Log.Error("Error on Execute_GameEnd RoomKind:{0}, MapNum:{1},\r\n{2}", room.RoomKindID, room.PlayingMapNum, ex.ToString());
			}
		}

		private static void Calc_DropItem(Account User, NormalRoom room, byte rank, float realrank, float rankmultiply)
		{
			try
			{
				bool flag = room.DropItem.ContainsKey(User.UserNum);
				if (User.Attribute == 3 || flag)
				{
					return;
				}
				CGameResultDesc cGameResultDesc = new CGameResultDesc();
				int num = 0;
				int num2 = 0;
				ushort BounsTR = 0;
				ushort BounsEXP = 0;
				int num3 = 0;
				float num4 = 10f;
				if (MapHolder.MapInfos.TryGetValue(room.PlayingMapNum, out var value))
				{
					num4 = value.RewardLengthRate;
				}
				else
				{
					Log.Warning("Invalid MapInfo MapNum:{0}", room.PlayingMapNum);
				}
				float raceDistance = User.RaceDistance;
				_ = User.RaceDistance;
				float num5 = room.MapMaxDistance;
				_ = room.MapMaxDistance;
				if (room.MapMaxDistance == 0f)
				{
					num5 = 1000f;
					Log.Warning("Room MapMaxDistance is null Map:{0}", room.PlayingMapNum);
				}
				bool flag2 = raceDistance >= num5 * num4 || rank > 100 || room.RuleType == 45568 || room.RuleType == 111104;
				if (room.RuleType != 256)
				{
					if (flag2)
					{
						int baseTR;
						int baseEXP;
						if (room.GameMode == 3)
						{
							GameLogic.getRelayMode_baseTREXP(User, room, rank, (int)realrank, out baseTR, out baseEXP);
						}
						else
						{
							GameLogic.getNormalMode_baseTREXP(User, room, rank, (int)realrank, out baseTR, out baseEXP);
						}
						num = Calc_TR(User, room, (byte)((rank > 10) ? 10 : rank), rankmultiply, baseTR, out BounsTR);
						num2 = Calc_EXP(User, room, (byte)((rank > 10) ? 10 : rank), rankmultiply, baseEXP, out BounsEXP);
					}
				}
				else
				{
					if (room.UserBounsItemInfos.TryGetValue(User.RoomPos, 1, out var value2))
					{
						num = value2;
					}
					if (room.UserBounsItemInfos.TryGetValue(User.RoomPos, 2, out var value3))
					{
						num2 = value3;
					}
					int num6 = 30000;
					if (User.getAttr(264, out var value4))
					{
						num6 += (int)value4;
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num > 30000)
					{
						num = 30000;
					}
					if (num2 > num6)
					{
						num2 = num6;
					}
					num3 = (int)Math.Round((float)(room.PlayerCount() - ((rank > room.PlayerCount()) ? room.PlayerCount() : rank) + 1) * ServerSettingHolder.ServerSettings.dungeonRaidGotPointConst + (float)ServerSettingHolder.ServerSettings.dungeonRaidBalanceConst, MidpointRounding.AwayFromZero);
					if (room.Channel == 38)
					{
						AssaultModeHandle.DungeonRaidAddPoint(User.UserNum, num3);
					}
				}
				int level = User.Level;
				int guildMatchPoint = 0;
				int num7 = 2;
				int num8 = 0;
				if (room.RoomKindID == 81 || room.RoomKindID == 82 || room.RoomKindID == 83)
				{
					int num9 = ((rank > 9) ? 9 : rank);
					num3 = (num7 = (guildMatchPoint = 25 - (num9 * 3 - 3)));
					if (User.getAttr(178, out var value5))
					{
						num7 += (int)value5;
					}
				}
				else if (flag2 && room.GameMode == 39)
				{
					int num10 = room.PlayerCount();
					float num11 = ((num10 > 8) ? 8 : num10);
					int num12 = 10 * num10 * -1;
					num8 = (num3 = (int)Math.Round((float)(10 * num10) * (1f - (realrank / num11 - 0.1f) * (realrank / num11)), MidpointRounding.AwayFromZero));
					if (num8 < num12)
					{
						num8 = (num3 = num12);
					}
					if (ThankOfferingSystem.ThankOfferingSchedule.TryGetValue(ServerSettingHolder.ServerSettings.ThankOfferingSchedule_CurNum, out var value6) && (!(value6.StartTime <= DateTime.Now) || !(DateTime.Now <= value6.EndTime)))
					{
						num8 = 0;
					}
				}
				else if (flag2 && room.isCompetitionEventMode())
				{
					int num13 = (int)((float)room.PlayerCount() - (realrank - 1f));
					num13 = ((num13 >= 0) ? num13 : 0);
					float num14 = 60f + (float)num13 * 1.5f;
					if (User.getAttr(238, out var value7))
					{
						value7 /= 100f;
						num14 += num14 * value7;
					}
					num3 = (int)Math.Round(num14, MidpointRounding.AwayFromZero);
					competitionEvent_addPoint(User.UserNum, num3);
				}
				GetGameReward(User, room, rank, out var rewardresult);
				GameResultPlayer(User, room, num2, num, guildMatchPoint, num7, num8);
				User.AgentConnect.Tell(new UpdateUserInfo
				{
					Session = User.Session,
					GameMoney = User.TR,
					EXP = User.Exp
				});
				bool flag3 = LobbyHandle.LevelUPCheck_RM(User, level);
				if (flag3)
				{
					room.LevelUPPlayerList.Add(User);
					cGameResultDesc.SetLevelUP();
				}
				MapCardHolder.GetCardProc(User.UserNum, flag2, room.PlayingMapNum, (float)User.Luck, out var takecard);
				if (BounsTR > 0)
				{
					cGameResultDesc.SetItemTRBonus();
				}
				if (BounsEXP > 0)
				{
					cGameResultDesc.SetItemEXPBonus();
				}
				bool flag4 = User.activeItem.HasPosition((ushort)812);
				if (flag4)
				{
					cGameResultDesc.SetFreePassBuffBonus();
				}
				DropList value8 = new DropList
				{
					UserNum = User.UserNum,
					NickName = User.NickName,
					FreePassType = (flag4 ? 2 : 4),
					TotalEXP = User.Exp,
					RaceDistance = raceDistance,
					ServerLapTime = User.ServerLapTime,
					LapTime = User.LapTime,
					Pos = User.RoomPos,
					Team = User.Team,
					RelayTeam = User.RelayTeam,
					RelayTeamPos = User.RelayTeamPos,
					BounsTR = BounsTR,
					BounsEXP = BounsEXP,
					TR = num,
					EXP = num2,
					Rank = User.Rank,
					isLevelUP = flag3,
					CardID = takecard,
					RewardItemID = rewardresult,
					MiniGamePoint = 0,
					MiniGameStarPoint = num3,
					TotalDamage = User.TotalDamage,
					MaxDamage = User.MaxDamage,
					GuildInfo = User.GuildInfo,
					GameResultDesc = cGameResultDesc
				};
				room.DropItem.TryAdd(User.UserNum, value8);
			}
			catch (Exception ex)
			{
				Log.Error("Calc DropItem Map:{0} Error:{1}", room.PlayingMapNum, ex.ToString());
			}
		}

		private static void Calc_BonusStageDropItem(Account User, NormalRoom room, byte rank)
		{
			bool flag = room.DropItem.ContainsKey(User.UserNum);
			if (User.Attribute != 3 && !flag)
			{
				int num = ServerSettingHolder.ServerSettings.BonusStage_Reward_TR;
				int num2 = ServerSettingHolder.ServerSettings.BonusStage_Reward_Exp;
				ushort bounsTR = 0;
				ushort bounsEXP = 0;
				bool flag2 = rank < 98;
				if (!flag2)
				{
					num = 0;
					num2 = 0;
				}
				int level = User.Level;
				int guildMatchPoint = 0;
				int guildPoint = 2;
				GetBonusStageGameReward(User, room, rank, out var rewardresult);
				GameResultPlayer(User, room, num2, num, guildMatchPoint, guildPoint, 0);
				User.AgentConnect.Tell(new UpdateUserInfo
				{
					Session = User.Session,
					GameMoney = User.TR,
					EXP = User.Exp
				});
				bool flag3 = LobbyHandle.LevelUPCheck_RM(User, level);
				if (flag3)
				{
					room.LevelUPPlayerList.Add(User);
				}
				MapCardHolder.GetCardProc(User.UserNum, flag2, room.PlayingMapNum, (float)User.Luck, out var takecard);
				bool flag4 = User.activeItem.HasPosition((ushort)812);
				DropList value = new DropList
				{
					UserNum = User.UserNum,
					NickName = User.NickName,
					FreePassType = (flag4 ? 2 : 4),
					TotalEXP = User.Exp,
					RaceDistance = User.RaceDistance,
					ServerLapTime = User.ServerLapTime,
					LapTime = User.LapTime,
					Pos = User.RoomPos,
					Team = User.Team,
					RelayTeam = User.RelayTeam,
					RelayTeamPos = User.RelayTeamPos,
					BounsTR = bounsTR,
					BounsEXP = bounsEXP,
					TR = num,
					EXP = num2,
					Rank = User.Rank,
					isLevelUP = flag3,
					CardID = takecard,
					RewardItemID = rewardresult,
					MiniGamePoint = 0,
					MiniGameStarPoint = 0,
					TotalDamage = User.TotalDamage,
					MaxDamage = User.MaxDamage,
					GuildInfo = User.GuildInfo
				};
				room.DropItem.TryAdd(User.UserNum, value);
			}
		}

		private static int Calc_TR(Account User, NormalRoom room, byte rank, float rankmultiply, int baseTR, out ushort BounsTR)
		{
			int num = room.PlayerCount();
			float multiplyTR = ServerSettingHolder.ServerSettings.MultiplyTR;
			int num2 = ((baseTR != 0) ? ((room.GameMode != 5) ? baseTR : (12 * num)) : ((room.GameMode != 5) ? ((int)Math.Round((float)(12 * num) * (float)(1m - ((decimal)rank / 10m - 0.1m)), MidpointRounding.AwayFromZero)) : (12 * num)));
			int num3 = 0;
			float num4 = 0f;
			float num5 = 0f;
			int num6 = 30000;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			foreach (KeyValuePair<short, float> getAttr in User.getAttrs)
			{
				switch (getAttr.Key)
				{
				case 1:
					num4 += getAttr.Value;
					break;
				case 14:
					if (room.IsTeamPlay == 0 && rank <= 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 30:
					if (room.IsTeamPlay == 0 && room.GameMode != 3)
					{
						num4 += (float)random.Next(Convert.ToInt32(getAttr.Value)) / 100f;
					}
					break;
				case 32:
					if (room.IsTeamPlay != 0 || room.GameMode == 3)
					{
						num4 += (float)random.Next(Convert.ToInt32(getAttr.Value)) / 100f;
					}
					break;
				case 37:
					num5 += getAttr.Value;
					break;
				case 51:
					num5 += (float)random.Next(Convert.ToInt32(getAttr.Value));
					break;
				case 91:
					if (room.Channel == 2)
					{
						num4 += getAttr.Value;
					}
					break;
				case 93:
					if (room.Channel == 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 103:
					if (room.Channel == 2)
					{
						num5 += getAttr.Value;
					}
					break;
				case 105:
					if (room.Channel == 3)
					{
						num5 += getAttr.Value;
					}
					break;
				case 131:
					if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 237:
					if (room.isCompetitionEventMode())
					{
						num4 += getAttr.Value / 100f;
					}
					break;
				case 266:
					num4 += getAttr.Value / 100f;
					break;
				}
			}
			num3 = (int)Math.Round(((float)num2 + (float)num2 * num4 + num5) * rankmultiply * multiplyTR, MidpointRounding.AwayFromZero);
			num3 = ((num3 > num6) ? num6 : num3);
			BounsTR = (ushort)(num3 - num2);
			return num3;
		}

		private static int Calc_EXP(Account User, NormalRoom room, byte rank, float rankmultiply, int baseEXP, out ushort BounsEXP)
		{
			int num = room.PlayerCount();
			float multiplyEXP = ServerSettingHolder.ServerSettings.MultiplyEXP;
			int num2 = ((baseEXP != 0) ? ((room.GameMode != 5) ? baseEXP : (12 * num)) : ((room.GameMode != 5) ? ((int)Math.Round((float)(12 * num) * (float)(1m - ((decimal)rank / 10m - 0.1m)), MidpointRounding.AwayFromZero)) : (12 * num)));
			int num3 = 0;
			float num4 = 0f;
			float num5 = 0f;
			int num6 = 30000;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			foreach (KeyValuePair<short, float> getAttr in User.getAttrs)
			{
				switch (getAttr.Key)
				{
				case 2:
					num4 += getAttr.Value;
					break;
				case 14:
					if (room.IsTeamPlay == 0 && rank <= 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 31:
					if (room.IsTeamPlay == 0 && room.GameMode != 3)
					{
						num4 += (float)random.Next(Convert.ToInt32(getAttr.Value)) / 100f;
					}
					break;
				case 33:
					if (room.IsTeamPlay != 0 || room.GameMode == 3)
					{
						num4 += (float)random.Next(Convert.ToInt32(getAttr.Value)) / 100f;
					}
					break;
				case 38:
					num5 += getAttr.Value;
					break;
				case 52:
					num5 += (float)random.Next(Convert.ToInt32(getAttr.Value));
					break;
				case 92:
					if (room.Channel == 2)
					{
						num4 += getAttr.Value;
					}
					break;
				case 94:
					if (room.Channel == 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 104:
					if (room.Channel == 2)
					{
						num5 += getAttr.Value;
					}
					break;
				case 106:
					if (room.Channel == 3)
					{
						num5 += getAttr.Value;
					}
					break;
				case 132:
					if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
					{
						num4 += getAttr.Value;
					}
					break;
				case 204:
					num5 += getAttr.Value;
					break;
				case 236:
					if (room.isCompetitionEventMode())
					{
						num4 += getAttr.Value / 100f;
					}
					break;
				case 264:
					num6 += (int)getAttr.Value;
					break;
				case 267:
					num4 += getAttr.Value / 100f;
					break;
				case 289:
					num4 += getAttr.Value / 100f;
					break;
				}
			}
			if (room.BuffType > 0)
			{
				num4 += 0.2f;
			}
			num3 = (int)Math.Round(((float)num2 + (float)num2 * num4 + num5) * rankmultiply * multiplyEXP, MidpointRounding.AwayFromZero);
			num3 = ((num3 > num6) ? num6 : num3);
			BounsEXP = (ushort)(num3 - num2);
			return num3;
		}

		private static void GetGameReward(Account User, NormalRoom room, int rank, out List<GameRewardResult> rewardresult)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			string text4 = string.Empty;
			string text5 = string.Empty;
			rewardresult = new List<GameRewardResult>();
			if (room.GameMode == 14 && (room.GameMode != 14 || User.GameEndType != 1))
			{
				return;
			}
			try
			{
				Random random = new Random(Guid.NewGuid().GetHashCode());
				foreach (KeyValuePair<int, short> rewardGroup in room.RewardGroupList)
				{
					if (!GameRewardHolder.SubGroupInfos.TryGetValue(rewardGroup.Key, out var value))
					{
						continue;
					}
					if ((float)value.TotalWeight <= 1000f)
					{
						GameRewardSubGroupInfo gameRewardSubGroupInfo = value.NextWithReplacement();
						if (!(User.RaceDistance >= room.MapMaxDistance * gameRewardSubGroupInfo.RaceRate) || !GameRewardHolder.GroupRateInfos.TryGetValue(rewardGroup.Key, gameRewardSubGroupInfo.SubGroup, out var value2))
						{
							continue;
						}
						double num = value2.TotalWeight;
						if (random.NextDouble() * 100000.0 + 1.0 <= num)
						{
							GameRewardGroupRate gameRewardGroupRate = value2.NextWithReplacement();
							text += $"{gameRewardGroupRate.GroupNum},";
							text2 += $"{gameRewardGroupRate.SubGroup},";
							text3 += $"{gameRewardGroupRate.RewardType},";
							text4 += $"{gameRewardGroupRate.RewardID},";
							text5 += $"{gameRewardGroupRate.Amount},";
							if (random.NextDouble() * 100.0 + 1.0 <= (double)gameRewardSubGroupInfo.OnePlusOneRate && User.FreePassType == 2)
							{
								text += $"{gameRewardGroupRate.GroupNum},";
								text2 += $"{gameRewardGroupRate.SubGroup},";
								text3 += $"{gameRewardGroupRate.RewardType},";
								text4 += $"{gameRewardGroupRate.RewardID},";
								text5 += $"{gameRewardGroupRate.Amount},";
							}
						}
						continue;
					}
					foreach (GameRewardSubGroupInfo item2 in value)
					{
						if (!(User.RaceDistance >= room.MapMaxDistance * item2.RaceRate) || !GameRewardHolder.GroupRateInfos.TryGetValue(rewardGroup.Key, item2.SubGroup, out var value3))
						{
							continue;
						}
						double num2 = value3.TotalWeight;
						if (random.NextDouble() * 100000.0 + 1.0 <= num2)
						{
							GameRewardGroupRate gameRewardGroupRate2 = value3.NextWithReplacement();
							text += $"{gameRewardGroupRate2.GroupNum},";
							text2 += $"{gameRewardGroupRate2.SubGroup},";
							text3 += $"{gameRewardGroupRate2.RewardType},";
							text4 += $"{gameRewardGroupRate2.RewardID},";
							text5 += $"{gameRewardGroupRate2.Amount},";
							if (random.NextDouble() * 100.0 + 1.0 <= (double)item2.OnePlusOneRate && User.FreePassType == 2)
							{
								text += $"{gameRewardGroupRate2.GroupNum},";
								text2 += $"{gameRewardGroupRate2.SubGroup},";
								text3 += $"{gameRewardGroupRate2.RewardType},";
								text4 += $"{gameRewardGroupRate2.RewardID},";
								text5 += $"{gameRewardGroupRate2.Amount},";
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on draw gamereward:{0}", ex.Message);
			}
			if (string.IsNullOrEmpty(text3))
			{
				return;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_gameReward_GiveReward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("rewardGroupNumList", MySqlDbType.VarString).Value = text;
				mySqlCommand.Parameters.Add("rewardSubGroupList", MySqlDbType.VarString).Value = text2;
				mySqlCommand.Parameters.Add("rewardTypeList", MySqlDbType.VarString).Value = text3;
				mySqlCommand.Parameters.Add("rewardIDList", MySqlDbType.VarString).Value = text4;
				mySqlCommand.Parameters.Add("rewardAmountList", MySqlDbType.VarString).Value = text5;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GameRewardResult item = new GameRewardResult
					{
						RewardType = Convert.ToInt16(mySqlDataReader["rewardType"]),
						RewardID = Convert.ToInt32(mySqlDataReader["rewardID"]),
						RewardAmount = Convert.ToInt32(mySqlDataReader["rewardAmount"])
					};
					rewardresult.Add(item);
				}
			}
			catch (Exception ex2)
			{
				Log.Error("Error on give gamereward:\r\n{0}, UserNum:{2}, rewardIDList:{1}", ex2.Message, text4, User.UserNum);
			}
		}

		private static void GetBonusStageGameReward(Account User, NormalRoom room, int rank, out List<GameRewardResult> rewardresult)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			string text4 = string.Empty;
			string text5 = string.Empty;
			rewardresult = new List<GameRewardResult>();
			if (room.BonusStageRewardInfo.TryGetValue(rank, out var value))
			{
				text += $"{100},";
				text2 += $"{100},";
				text3 += $"{value.RewardType},";
				text4 += $"{value.RewardID},";
				text5 += $"{value.RewardAmount},";
			}
			if (string.IsNullOrEmpty(text3))
			{
				return;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_gameReward_GiveReward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("rewardGroupNumList", MySqlDbType.VarString).Value = text;
				mySqlCommand.Parameters.Add("rewardSubGroupList", MySqlDbType.VarString).Value = text2;
				mySqlCommand.Parameters.Add("rewardTypeList", MySqlDbType.VarString).Value = text3;
				mySqlCommand.Parameters.Add("rewardIDList", MySqlDbType.VarString).Value = text4;
				mySqlCommand.Parameters.Add("rewardAmountList", MySqlDbType.VarString).Value = text5;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GameRewardResult item = new GameRewardResult
					{
						RewardType = Convert.ToInt16(mySqlDataReader["rewardType"]),
						RewardID = Convert.ToInt32(mySqlDataReader["rewardID"]),
						RewardAmount = Convert.ToInt32(mySqlDataReader["rewardAmount"])
					};
					rewardresult.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on give gamereward:\r\n{0}, UserNum:{2}, rewardIDList:{1}", ex.Message, text4, User.UserNum);
			}
		}

		private static byte[] GenResult(NormalRoom room, byte last)
		{
			PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
			packetWriter.WriteOP(RoomOpcodes.eRoom_GAME_RESULT_RANK);
			packetWriter.Write((byte)room.RoomKindID);
			bool flag = room.IsTeamPlay != 0;
			bool flag2 = room.GameMode == 5;
			packetWriter.Fill(8);
			packetWriter.Write(flag2);
			if (flag2)
			{
				packetWriter.Write(room.CorunModeResult, 0, room.CorunModeResult.Length);
				room.CorunModeResult = null;
			}
			packetWriter.Write((byte)0);
			packetWriter.Write(flag);
			if (flag)
			{
				int team = (from p in room.DropItem.Values
					orderby p.Rank, p.RaceDistance descending
					select p).FirstOrDefault().Team;
				int value = ((team != 1) ? 1 : 2);
				packetWriter.Write(2);
				packetWriter.Write(team);
				packetWriter.Write(value);
				packetWriter.Write(team);
			}
			int count = room.DropItem.Count;
			packetWriter.Write((byte)count);
			foreach (DropList item in (from p in room.DropItem.Values
				orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
				select p).ThenBy((DropList o) => o.Pos))
			{
				if (item.GuildInfo == null)
				{
					packetWriter.Fill(6);
				}
				else
				{
					packetWriter.Write((short)item.GuildInfo.level);
					packetWriter.Write(item.GuildInfo.guildNum);
				}
				packetWriter.Write(item.ServerLapTime);
				packetWriter.Write(item.LapTime);
				packetWriter.Write(item.Rank);
				packetWriter.Write(item.Pos);
				packetWriter.Write(item.EXP);
				packetWriter.Write(item.MiniGameStarPoint);
				packetWriter.Write(item.TR);
				packetWriter.Write(0);
				packetWriter.Write(0);
				packetWriter.Write(0);
				if (room.IsTeamPlay != 0)
				{
					packetWriter.Write((int)item.Team);
				}
				packetWriter.Write((int)item.RelayTeamPos);
				packetWriter.Write(item.BounsEXP);
				packetWriter.Write(item.BounsTR);
				packetWriter.Write(item.GameResultDesc.GetRawData());
				packetWriter.Write(count);
				int count2 = item.CardID.Count;
				packetWriter.Write(count2);
				foreach (int item2 in item.CardID)
				{
					packetWriter.Write(item2);
				}
				packetWriter.Write(428587520);
				packetWriter.Fill(20);
				packetWriter.Write(464569600);
			}
			byte[] nickname;
			bool flag3 = SendRegisterGoods(room, out nickname, last);
			if (flag3)
			{
				Account account = room.Players.Values.FirstOrDefault((Account p) => p.RoomPos == room.RoomMasterIndex);
				account.SendAsync(new GameRoom_DeleteKeepItem(account, room, last));
			}
			if (room.GameMode == 3)
			{
				int num = 1;
				IEnumerable<byte> enumerable = (from o in room.DropItem.Values
					orderby o.Rank
					select o into s
					select s.RelayTeam).Distinct();
				packetWriter.Write(enumerable.Count());
				foreach (byte item3 in enumerable)
				{
					packetWriter.Write(num);
					packetWriter.Write(item3);
					num++;
				}
			}
			else
			{
				packetWriter.Write(0);
			}
			if (room.RuleType != 256)
			{
				packetWriter.Write(0);
			}
			else
			{
				packetWriter.Write(room.DropItem.Count);
				foreach (DropList item4 in room.DropItem.Values.OrderByDescending((DropList o) => o.TotalDamage))
				{
					packetWriter.WriteAnsiFixed_intSize(item4.NickName);
					packetWriter.Write(item4.Rank >= 98);
					packetWriter.Write(item4.TotalDamage);
					packetWriter.Write(item4.MaxDamage);
					packetWriter.Write(0);
					packetWriter.Write(item4.RewardItemID.Count);
					foreach (GameRewardResult item5 in item4.RewardItemID)
					{
						packetWriter.Write(item5.RewardID);
					}
				}
			}
			packetWriter.Write(2);
			packetWriter.Write(1002);
			packetWriter.Write((!flag3) ? 17 : (17 + nickname.Length));
			packetWriter.Write(room.ItemNum);
			if (!flag3)
			{
				packetWriter.Write(-1L);
				packetWriter.Write((byte)0);
				packetWriter.Write(1);
			}
			else
			{
				packetWriter.Write(3873842);
				packetWriter.Write(2);
				packetWriter.Write(nickname, 0, nickname.Length);
				packetWriter.Write((byte)0);
				packetWriter.Write(nickname.Length + 1);
			}
			packetWriter.Write(1014);
			packetWriter.Write(4);
			packetWriter.Write(0);
			packetWriter.Write(last);
			byte[] result = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			packetWriter = null;
			return result;
		}

		private static void CorunModeResult(NormalRoom room, out float rankmultiply)
		{
			rankmultiply = 1f;
			try
			{
				PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
				ConcurrentDictionary<int, List<CorunModeResult>> value;
				bool num = GameModeHolder.CorunModeInfos.TryGetValue(room.PlayingMapNum, out value);
				bool flag = room.PlayerList().Any((Account a) => a.GameEndType == 1);
				int value2 = room.ClearAreaTime.Count + (flag ? 4 : 3);
				packetWriter.Write(value2);
				short num2 = 0;
				packetWriter.Write(0);
				packetWriter.Write((int)room.Survival);
				int num3 = room.Survival * 10;
				num2 = (short)(num2 + (short)num3);
				packetWriter.Write(num3);
				int value3;
				bool flag2 = room.ClearAreaTime.TryGetValue(100, out value3);
				int num4 = (flag ? (flag2 ? (room.StartKillBossRemainTime - value3) : room.StartKillBossRemainTime) : 0);
				CorunModeResult corunModeResult = null;
				corunModeResult = ((!num) ? new CorunModeResult
				{
					ResultPoint = 0,
					ResultType = 0,
					TimeFrom = 0,
					TimeTo = 0
				} : GameModeHolder.GetResultInfo(value, 1, num4));
				packetWriter.Write(corunModeResult.ResultType);
				packetWriter.Write(num4);
				num2 = (short)(num2 + corunModeResult.ResultPoint);
				packetWriter.Write(corunModeResult.ResultPoint);
				packetWriter.Write((ushort)60144);
				packetWriter.Write((byte)46);
				int num5 = 0;
				foreach (KeyValuePair<byte, int> item in room.ClearAreaTime)
				{
					int num6 = ((item.Key != 100) ? (item.Key + 10) : 2);
					try
					{
						num5 += item.Value;
						CorunModeResult resultInfo = GameModeHolder.GetResultInfo(value, num6, item.Value);
						packetWriter.Write(resultInfo.ResultType);
						packetWriter.Write(item.Value);
						num2 = (short)(num2 + resultInfo.ResultPoint);
						packetWriter.Write(resultInfo.ResultPoint);
						packetWriter.Write((ushort)52197);
						packetWriter.Write((byte)15);
					}
					catch (Exception ex)
					{
						Log.Error("room.ClearAreaTime Error:{0}, room.PlayingMapNum:{1}, resulttype:{2}", ex.Message, room.PlayingMapNum, num6);
					}
				}
				if (flag)
				{
					packetWriter.Write(250);
					packetWriter.Write(num5);
					packetWriter.Write((byte)0);
					packetWriter.Write((ushort)52197);
					packetWriter.Write((byte)15);
				}
				CorunModeResult resultInfo2 = GameModeHolder.GetResultInfo(value, 3, 0);
				packetWriter.Write(resultInfo2.ResultType);
				packetWriter.Write(room.PlayingMapNum);
				num2 = (short)(num2 + resultInfo2.ResultPoint);
				packetWriter.Write(resultInfo2.ResultPoint);
				packetWriter.Write((ushort)52196);
				packetWriter.Write((byte)15);
				packetWriter.Write(num2);
				short value4 = 0;
				if (num2 >= 501)
				{
					value4 = 5;
					rankmultiply = 3f;
				}
				else if (num2 >= 401)
				{
					value4 = 4;
					rankmultiply = 2.5f;
				}
				else if (num2 >= 301)
				{
					value4 = 3;
					rankmultiply = 2f;
				}
				else if (num2 >= 201)
				{
					value4 = 2;
					rankmultiply = 1.5f;
				}
				else if (num2 >= 151)
				{
					value4 = 1;
					rankmultiply = 1f;
				}
				else if (num2 >= 0)
				{
					value4 = 0;
					rankmultiply = 0.5f;
				}
				if (!flag)
				{
					rankmultiply = 0.2f;
				}
				packetWriter.Write(value4);
				room.CorunModeResult = packetWriter.ToArray();
				PacketWriter.ReleaseInstance(packetWriter);
				packetWriter = null;
			}
			catch (Exception ex2)
			{
				Log.Error("CorunModeResult Error:{0}, room.PlayingMapNum:{1}", ex2.Message, room.PlayingMapNum);
			}
		}

		public static bool SendRegisterGoods(NormalRoom room, out byte[] nickname, byte last)
		{
			if (room.ItemNum != -1)
			{
				if (room.isOrderBy != 1)
				{
					Account account = room.Players.Values.OrderBy((Account _) => Guid.NewGuid()).FirstOrDefault((Account f) => f.RoomPos != room.RoomMasterIndex);
					if (account == null)
					{
						if (room.Players.TryGetValue(room.RoomMasterIndex, out var value))
						{
							value.SendAsync(new GameRoom_LockKeepItem(room, isCancel: true, last));
						}
						nickname = null;
						return false;
					}
					StorageGiveReward(account.UserNum, room.ItemNum, room.Storage_Id);
					nickname = Encoding.Default.GetBytes(account.NickName);
					return true;
				}
				foreach (Account item in room.PlayerList())
				{
					if (item.Rank == room.SendRank)
					{
						if (item.RoomPos == room.RoomMasterIndex)
						{
							item.SendAsync(new GameRoom_LockKeepItem(room, isCancel: true, last));
							nickname = null;
							return false;
						}
						StorageGiveReward(item.UserNum, room.ItemNum, room.Storage_Id);
						nickname = Encoding.Default.GetBytes(item.NickName);
						return true;
					}
				}
			}
			nickname = null;
			return false;
		}

		private static bool StorageGiveReward(int recvUserNum, int ItemNum, long uniqueNum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_storage_giveroomreward";
				mySqlCommand.Parameters.Add("recvUserNum", MySqlDbType.Int32).Value = recvUserNum;
				mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
				mySqlCommand.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = uniqueNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToByte(mySqlDataReader["retval"]) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public static void GameResultPlayer(Account User, NormalRoom room, int gotexp, int gottr, int guildMatchPoint, int guildPoint, int ladderpoint)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_gameresultPlayer";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("exp", MySqlDbType.Int32).Value = gotexp;
				mySqlCommand.Parameters.Add("gamemoney", MySqlDbType.Int32).Value = gottr;
				mySqlCommand.Parameters.Add("roomKind", MySqlDbType.Int16).Value = room.RoomKindID;
				mySqlCommand.Parameters.Add("mapNum", MySqlDbType.Int32).Value = room.PlayingMapNum;
				mySqlCommand.Parameters.Add("bAssaultMode", MySqlDbType.Int16).Value = 0;
				mySqlCommand.Parameters.Add("timeOut", MySqlDbType.Int32).Value = User.Rank >= 98;
				mySqlCommand.Parameters.Add("bWinTeam", MySqlDbType.Int32).Value = 0;
				mySqlCommand.Parameters.Add("rank", MySqlDbType.Int32).Value = User.Rank;
				mySqlCommand.Parameters.Add("raceRate", MySqlDbType.Int32).Value = User.RaceDistance;
				mySqlCommand.Parameters.Add("lapTime", MySqlDbType.Int32).Value = User.LapTime;
				mySqlCommand.Parameters.Add("playTime", MySqlDbType.Int32).Value = User.ServerLapTime;
				mySqlCommand.Parameters.Add("plevel", MySqlDbType.Int32).Value = User.Level;
				mySqlCommand.Parameters.Add("guildMatchPoint", MySqlDbType.Int32).Value = guildMatchPoint;
				mySqlCommand.Parameters.Add("guildPoint", MySqlDbType.Int32).Value = guildPoint;
				mySqlCommand.Parameters.Add("ladderpoint", MySqlDbType.Int32).Value = ladderpoint;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				User.Exp = mySqlDataReader.GetInt64("exp");
				User.TR = mySqlDataReader.GetInt64("gameMoney");
				User.LadderPoint = Convert.ToInt32(mySqlDataReader["ladderpoint"]);
			}
			catch (Exception ex)
			{
				Log.Error("UserNum:{0} usp_gameresultPlayer Error: {1}", User.UserNum, ex.Message);
			}
		}

		private static void guildMatchRecord(int winGuildNum, string winGuildName, int successiveWin, int winTeamLadderPoint, int loseGuildNum, string loseGuildName, int loseTeamLadderPoint, short matchType)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMatchRecord";
				mySqlCommand.Parameters.Add("winGuildNum", MySqlDbType.Int32).Value = winGuildNum;
				mySqlCommand.Parameters.Add("winGuildName", MySqlDbType.VarString).Value = winGuildName;
				mySqlCommand.Parameters.Add("successiveWin", MySqlDbType.Int32).Value = successiveWin;
				mySqlCommand.Parameters.Add("winTeamLadderPoint", MySqlDbType.Int32).Value = winTeamLadderPoint;
				mySqlCommand.Parameters.Add("loseGuildNum", MySqlDbType.Int32).Value = loseGuildNum;
				mySqlCommand.Parameters.Add("loseGuildName", MySqlDbType.VarString).Value = loseGuildName;
				mySqlCommand.Parameters.Add("loseTeamLadderPoint", MySqlDbType.Int32).Value = loseTeamLadderPoint;
				mySqlCommand.Parameters.Add("matchType", MySqlDbType.Int16).Value = matchType;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMatchRecord Error: {0}", ex.Message);
			}
		}

		private static void competitionEvent_addPoint(int userNum, int point)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_competitionEvent_addPoint";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				mySqlCommand.Parameters.Add("point", MySqlDbType.Int64).Value = point;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_competitionEvent_addPoint Error: {0}", ex.Message);
			}
		}

		public static void VertificationBan(Account User)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_insertBlackList";
			mySqlCommand.Parameters.Add("commandusernum", MySqlDbType.Int32).Value = 0;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = User.NickName;
			mySqlCommand.Parameters.Add("blockreason", MySqlDbType.Int32).Value = 6;
			mySqlCommand.Parameters.Add("blocktime", MySqlDbType.Int32).Value = 35;
			mySqlCommand.Parameters.Add("remoteIP", MySqlDbType.VarString).Value = User.LastIp;
			using (mySqlCommand.ExecuteReader(CommandBehavior.SingleRow))
			{
			}
		}

		public static void HackingBan(Account User)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_insertBlackList";
			mySqlCommand.Parameters.Add("commandusernum", MySqlDbType.Int32).Value = 0;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = User.NickName;
			mySqlCommand.Parameters.Add("blockreason", MySqlDbType.Int32).Value = 1;
			mySqlCommand.Parameters.Add("blocktime", MySqlDbType.Int32).Value = 10;
			mySqlCommand.Parameters.Add("remoteIP", MySqlDbType.VarString).Value = User.LastIp;
			using (mySqlCommand.ExecuteReader(CommandBehavior.SingleRow))
			{
			}
		}
	}
}
