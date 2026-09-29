using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using RoomServer.Holders;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.IceFlower;
using RoomServer.Structuring.ItemRacing;
using RoomServer.Structuring.Map;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.Room;
using RoomServer.Structuring.SubjectKing;
using RoomServer.Structuring.TypingRun;
using Serilog;

namespace RoomServer.Packet
{
	public class GameModeHandle
	{
		public static void GameMode_LapTimeCountdwon(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || !room.isPlaying)
			{
				return;
			}
			short second = reader.ReadLEInt16();
			byte round = reader.ReadByte();
			room.BroadcastToAll(new GameRoom_LapTimeCountdwon(second, round, last));
			if (room.GameMode == 38)
			{
				Task.Run(() => Task.Delay(second * 1000)).ContinueWith(delegate
				{
					foreach (Account item in room.PlayerList())
					{
						item.SendAsync(new GameRoom_LapTimeCountdwon2(room, second, round, flag: true, last));
						item.GameEndType = 0;
					}
					room.Round = round;
					if (room.Round == 0)
					{
						foreach (Account value2 in room.Players.Values)
						{
							DropList value = new DropList
							{
								UserNum = value2.UserNum,
								RaceDistance = value2.RaceDistance,
								ServerLapTime = value2.ServerLapTime,
								LapTime = value2.LapTime,
								Pos = value2.RoomPos,
								Team = value2.Team,
								BounsTR = 0,
								BounsEXP = 0,
								TR = 0,
								EXP = 0,
								Rank = 0,
								CardID = new List<int> { 0 },
								MiniGamePoint = 0,
								MiniGameStarPoint = 0,
								GuildInfo = value2.GuildInfo
							};
							GameRoomEvent.GameResultPlayer(value2, room, 0, 0, 0, 0, 0);
							room.DropItem.TryAdd(value2.UserNum, value);
						}
					}
				});
				return;
			}
			Task.Run(async delegate
			{
				long timelimit = Utility.CurrentTimeMilliseconds() + second * 1000;
				bool gameovered = false;
				while (room.isPlaying && room.PlayerList().Exists((Account p) => p.GameEndType == 0) && !gameovered)
				{
					if (Utility.CurrentTimeMilliseconds() >= timelimit)
					{
						foreach (Account item2 in from w in room.PlayerList()
							where w.GameEndType == 0
							select w)
						{
							item2.SendAsync(new GameRoom_LapTimeCountdwon2(room, second, round, flag: false, last));
						}
						gameovered = true;
					}
					await Task.Delay(100);
				}
			});
		}

		public static void GameMode_MiniGame_RoundTime(Account User, PacketReader reader, byte last)
		{
			int num = reader.ReadLEInt32();
			int isnextround = reader.ReadLEInt32();
			float num2 = reader.ReadLESingle();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			foreach (Account item in room.PlayerList())
			{
				item.SendAsync(new GameRoom_MiniGame_RoundTime(item, num, isnextround, num2, last));
				item.GameOver = false;
			}
			room.Survival = (byte)room.PlayerCount();
			long RoundEndTime = Utility.CurrentTimeMilliseconds() + (long)(num2 * 1000f);
			Task.Run(delegate
			{
				GameMode_MiniGame_RoundThread(RoundEndTime, room);
			});
			User.SendAsync(new GameRoom_MiniGame_602(User, num, last));
			room.RespwanList.Clear();
		}

		public static async void GameMode_MiniGame_RoundThread(long RoundTime, NormalRoom room)
		{
			while (Utility.CurrentTimeMilliseconds() < RoundTime && room.Survival != 0)
			{
			}
			int point = 100 + 50 * room.Round;
			if (room.PlayingMapNum != 40006)
			{
				(from w in room.Players.Values
					where w.Attribute != 3 && !w.GameOver
					select w into p
					join d in room.DropItem on p.UserNum equals d.Key
					select new { p, d }).ToList().ForEach(f =>
				{
					f.d.Value.MiniGamePoint += point;
				});
			}
			int isnextround = 1;
			foreach (Account item in room.PlayerList())
			{
				item.SendAsync(new GameRoom_MiniGame_RoundTime(item, room.Round, isnextround, 1f, 1));
				item.SendAsync(new GameRoom_MiniGame_UpdatePoint(room, 1));
			}
			int num = room.Round + 1;
			int num2 = ((room.PlayingMapNum == 40003) ? 1 : 3);
			if (num != num2)
			{
				return;
			}
			int num3 = 100;
			foreach (var item2 in from p in room.Players.Values
				where p.Attribute != 3
				join d in room.DropItem on p.UserNum equals d.Key
				select new { p, d } into o
				orderby o.d.Value.MiniGamePoint descending, o.p.RoomPos
				select o)
			{
				if (item2.d.Value.MiniGamePoint > 0)
				{
					item2.d.Value.Rank = room.Rank++;
					int num4 = num3++;
					item2.p.ServerLapTime = num4;
					item2.p.LapTime = num4;
					item2.d.Value.MiniGameStarPoint = (int)Math.Round((double)item2.d.Value.MiniGamePoint / 4.5, MidpointRounding.AwayFromZero);
					UpdateUserPoint(item2.p.UserNum, 2400, item2.d.Value.MiniGameStarPoint);
				}
				else
				{
					item2.d.Value.Rank = 98;
					item2.p.ServerLapTime = 100000;
					item2.p.LapTime = 100000;
				}
			}
			room.CouplePointUpdate();
			room.Result = GenResult_ForMiniGameMode(room, 1);
			room.RegisterItem(-1, -1L, 2, 273281036, ispublic: true);
			room.GameRoomResult();
			List<Account> playerlist = room.PlayerList();
			foreach (Account item3 in playerlist)
			{
				item3.SendAsync(new GameRoom_GameUpdateEXP_New(room.RoomKindID, item3, 1));
			}
			foreach (Account item4 in playerlist)
			{
				item4.SendAsync(new GameRoom_GameResult2(item4, room.Result));
				item4.SendAsync(new GameRoom_Result(room.RoomKindID, 1));
				foreach (Account item5 in playerlist)
				{
					item4.SendAsync(new GameRoom_RoomPosReady(item5.RoomPos, isReady: false, 1));
				}
				item4.SendAsync(new GameRoom_GoodsInfo(room, 1));
			}
			foreach (Account item6 in playerlist)
			{
				item6.SendAsync(new GameRoom_UpdateIndividualGameRecord(item6, 1));
			}
			foreach (Account item7 in room.PlayerList().FindAll((Account p) => p.NeedVertificated))
			{
				item7.SendAsync(new DisconnectPacket(item7, 270, 1));
				item7.isDisconnected = true;
				GameRoomEvent.VertificationBan(item7);
			}
			foreach (Account item8 in room.PlayerList())
			{
				item8.MatchTime++;
				if (item8.MatchTime % 10 == 0)
				{
					item8.VertificationCode = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(0, 100);
					item8.NeedVertificated = true;
				}
			}
			await Task.Delay(5000);
			foreach (Account item9 in playerlist)
			{
				item9.SendAsync(new MoveToGameRoom(1));
				if (item9.NeedVertificated)
				{
					item9.SendAsync(new GameRoom_GetVertificationInfo(item9, 1));
				}
			}
			room.GameEndSetNewRoomMaster();
			room.StartAutoChangeRoomMaster();
			room.GameEndReset();
		}

		public static void GameMode_MiniGame_GetPoint(Account User, PacketReader reader, byte last)
		{
			reader.ReadByte();
			int num = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room.DropItem.ContainsKey(User.UserNum))
			{
				int nowpoint = (room.DropItem[User.UserNum].MiniGamePoint += num);
				room.BroadcastToAll(new GameRoom_MiniGame_UpdatePoint(room, last));
				User.SendAsync(new GameRoom_MiniGame_GetPoint(User, nowpoint, num, last));
			}
		}

		public static void GameMode_GameOver(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null)
			{
				reader.Offset += 4;
				_ = reader.ReadLESingle() / 100f;
				room.GameOver(User, 2, last);
			}
		}

		public static void GameMode_TimeOver(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null && room.GameMode != 38)
			{
				reader.Offset += 4;
				_ = reader.ReadLESingle() / 100f;
				room.TimeOver(User, last);
			}
		}

		public static void GameMode_MiniGame_Respawn(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			room.RespwanList.Add(User.RoomPos);
			if (room.Players.Values.Count((Account p) => p.Attribute != 3 && p.GameOver) != room.RespwanList.Count)
			{
				return;
			}
			foreach (Account item in room.PlayerList())
			{
				foreach (byte respwan in room.RespwanList)
				{
					item.SendAsync(new GameRoom_MiniGame_Respawn(item, respwan, last));
				}
			}
		}

		public static void GameMode_FootStep_GoalIn(Account User, PacketReader reader, byte last)
		{
			Log.Debug("FootStep_GoalIn");
			if (User.GameEndType != 0)
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			reader.ReadByte();
			User.LapTime = num;
			User.ServerLapTime = room.GetCurrentTime();
			if (!room.isGoal)
			{
				room.isGoal = true;
				if (room.RuleType == 8)
				{
					foreach (Account item in room.PlayerList())
					{
						item.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
						item.SendAsync(new Amsan_LapTimeControl(room.GetCurrentTime(), 10000, 0, isCorrect: false, last));
						item.LastLapTime = User.ServerLapTime;
						item.CurrentLapTime = 10000;
					}
					User.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
				}
				else if (room.RuleType == 16)
				{
					foreach (Account item2 in room.PlayerList())
					{
						item2.SendAsync(new FootStep_GoalIn_CountDown(num, 10000, last));
						item2.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
						item2.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last));
					}
					User.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
				}
				else
				{
					foreach (Account item3 in room.PlayerList())
					{
						item3.SendAsync(new FootStep_GoalIn_CountDown(num, 10000, last));
						item3.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
						item3.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
						item3.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last));
					}
				}
				long EndTime = Utility.CurrentTimeMilliseconds() + 15000;
				Task.Run(delegate
				{
					GameRoomEvent.Execute_GameEnd(room, EndTime, last);
				});
				return;
			}
			if (room.RuleType == 8)
			{
				foreach (Account item4 in room.PlayerList())
				{
					item4.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
				}
				User.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
				return;
			}
			if (room.RuleType == 16)
			{
				foreach (Account item5 in room.PlayerList())
				{
					item5.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
				}
				User.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
				return;
			}
			foreach (Account item6 in room.PlayerList())
			{
				item6.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
				item6.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
			}
		}

		public static void GameMode_Amsan_LapTime(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int unk = reader.ReadLEInt32();
			byte round = reader.ReadByte();
			room.BroadcastToAll(new Amsan_LapTime(unk, round, User.RoomPos, last));
		}

		public static void GameMode_Amsan_StepButton(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int btnid = reader.ReadLEInt32();
			room.BroadcastToAll(new Amsan_Step_Button(btnid, User.RoomPos, last));
		}

		public static void GameMode_Amsan_StepButton_Push(Account User, PacketReader reader, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).BroadcastToAll(new Amsan_Step_Button_Push(last));
		}

		public static void GameMode_Amsan_FinalButton(Account User, PacketReader reader, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).BroadcastToAll(new Amsan_Goal_Button(last));
		}

		public static void GameMode_Amsan_LapTimeControl(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			int num2 = 15000;
			int num3 = User.CurrentLapTime - (num - User.LastLapTime);
			if (flag)
			{
				User.CurrentLapTime = num3 + ((!room.isGoal) ? num2 : 0);
			}
			else
			{
				User.CurrentLapTime = num3 - num2;
			}
			User.LastLapTime = num;
			User.SendAsync(new Amsan_LapTimeControl(num, User.CurrentLapTime, num2, flag, last));
		}

		public static void GameMode_RandomGameOver(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte channelCheckPoint = reader.ReadByte();
			room.RandomGameOverCheckPoint(channelCheckPoint, User.RoomPos, last);
		}

		public static void GameMode_RandomGameOver_Die(Account User, PacketReader reader, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).RandomGameOverNotify(reader.ReadByte(), racelen: reader.ReadLESingle(), Session: User.Session, last: last);
		}

		public static void GameMode_CatchFish(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			reader.ReadLEInt16();
			reader.ReadLEInt32();
			if (num2 == -1)
			{
				if (MapItemHolder.CapsuleItemMapJoints.TryGetValue(room.PlayingMapNum, out var value))
				{
					int groupNum = value.GroupNum;
					MapItemHolder.MapCapsuleItems.TryGetValue(groupNum, out var value2);
					num2 = ((room.RuleType != 16384) ? value2.NextWithReplacement().GameItemNum : value2.FirstOrDefault((MapCapsuleItemInfo w) => w.PresentRuleType == 6 && w.Argument == ((room.HumanTeam != User.Team) ? 1 : 2)).GameItemNum);
				}
				else
				{
					Log.Warning("MakeAndEatItem map num: {0} not exist group!", room.PlayingMapNum);
				}
			}
			if (num2 > 0)
			{
				room.DrawItem2(User, num + 100, bMakeAndEat: true, num2, last);
			}
		}

		public static void RunQuizMode_RequestQuizList(Account User, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).BroadcastToAll(new RequestQuizList_Ack(last));
		}

		public static void CorunMode_SetClearLimitTime(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int sec = reader.ReadLEInt32();
			room.SetLapTime(sec, last);
		}

		public static void CorunMode_ClearTimeSection(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte b = reader.ReadByte();
			int num = reader.ReadLEInt32();
			int sec = reader.ReadLEInt32();
			if (room.GameMode == 5 && !room.ClearAreaTime.ContainsKey(b))
			{
				room.ClearAreaTime.TryAdd(b, num);
			}
			room.BroadcastToAll(new ClearTimeSection_Ack(b, num, sec, last));
		}

		public static void CorunMode_EnterTimeSection(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte area = reader.ReadByte();
			int remaintime = reader.ReadLEInt32();
			int addsec = reader.ReadLEInt32();
			room.UpdateLapTime(area, remaintime, addsec, last);
		}

		public static void GameMode_TurtleEatItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			float fatigue = User.Fatigue;
			switch (num)
			{
			case 0:
				if (fatigue >= 10f)
				{
					User.Fatigue -= ServerSettingHolder.ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_DEC;
				}
				else
				{
					User.Fatigue = 0f;
				}
				break;
			case 1:
				if (fatigue <= 90f)
				{
					User.Fatigue += ServerSettingHolder.ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_INC;
				}
				else
				{
					User.Fatigue = 100f;
				}
				break;
			}
			User.SendAsync(new TeamStatus(User, room, last));
		}

		public static void GameMode_ReqChangeTeamLeader(Account User, byte last)
		{
			Account account = Rooms.GetRoom(User.CurrentRoomId).Players.Values.FirstOrDefault((Account p) => p.RoomPos == User.Partner);
			if (User.TeamLeader)
			{
				User.TeamLeader = false;
				account.TeamLeader = true;
				User.SendAsync(new ReqChangeTeamLeader(0, last));
				account.SendAsync(new ReqChangeTeamLeader(0, last));
				User.SendAsync(new NewTeamLeader(account.RoomPos, last));
				account.SendAsync(new NewTeamLeader(account.RoomPos, last));
			}
			else
			{
				User.SendAsync(new ReqChangeTeamLeader(4, last));
				account.SendAsync(new ReqChangeTeamLeader(4, last));
			}
		}

		public static void CorunMode_SetBossEnergy(Account User, PacketReader reader, byte last)
		{
			int num = reader.ReadLEInt32();
			int boss = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			room.CorunBossHP = num / ServerSettingHolder.ServerSettings.corunModeDecreaseEnergyRatio;
			room.BroadcastToAll(new CorunMode_SetBossEnergy_Ack(boss, last));
		}

		public static void CorunMode_DecreaseBossEnergy(Account User, PacketReader reader, byte last)
		{
			int num = reader.ReadLEInt32();
			int boss = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num2 = room.CorunBossHP - num;
			room.CorunBossHP = ((num2 >= 0) ? num2 : 0);
			room.BroadcastToAll(new CorunMode_DecreaseBossEnergy_Ack(boss, room.CorunBossHP, last));
		}

		public static void CorunMode_SetObjectBossEnergy(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int boss = reader.ReadLEInt32();
			long num = reader.ReadLEInt64();
			int num2 = reader.ReadLEInt32() / ServerSettingHolder.ServerSettings.corunModeDecreaseEnergyRatio;
			ObjectBoss value = new ObjectBoss
			{
				HP = num2,
				MaxHP = num2
			};
			room.ObjectBoss.TryAdd(num, value);
			room.BroadcastToAll(new CorunMode_SetObjectBossEnergy_Ack(boss, num, last));
		}

		public static void CorunMode_DecreaseObjectBossEnergy(Account User, PacketReader reader, byte last)
		{
			int boss = reader.ReadLEInt32();
			long num = reader.ReadLEInt64();
			int num2 = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num3 = room.ObjectBoss[num].HP - num2;
			room.ObjectBoss[num].HP = ((num3 >= 0) ? num3 : 0);
			room.BroadcastToAll(new CorunMode_DecreaseObjectBossEnergy_Ack(boss, num, room.ObjectBoss[num].HP, last));
		}

		public static void CorunMode_IncreaseObjectBossEnergy(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int boss = reader.ReadLEInt32();
			long num = reader.ReadLEInt64();
			int num2 = reader.ReadLEInt32();
			int num3 = room.ObjectBoss[num].HP + num2;
			room.ObjectBoss[num].HP = ((num3 > room.ObjectBoss[num].MaxHP) ? room.ObjectBoss[num].MaxHP : num3);
			room.BroadcastToAll(new CorunMode_IncreaseObjectBossEnergy_Ack(boss, num, room.ObjectBoss[num].HP, last));
		}

		public static void CorunMode_TriggerObjectEvent(Account User, PacketReader reader, byte last)
		{
			int unk = reader.ReadLEInt32();
			reader.ReadByte();
			byte b = reader.ReadByte();
			int unk2 = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			switch (b)
			{
			case 0:
				room.RideCattlePlayer.Add(User);
				break;
			case 1:
				room.RideCattlePlayer.Remove(User);
				break;
			}
			if (room.RideCattlePlayer.Count == room.PlayerCount() || b == 2)
			{
				room.BroadcastToAll(new TriggerObjectEvent_Ack(unk, b, unk2, last));
				room.RideCattlePlayer.Clear();
			}
		}

		public static void CorunMode_TriggerCheckInObjectEvent(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int needcount = reader.ReadLEInt32();
			int tookcount = reader.ReadLEInt32();
			reader.ReadLEInt16();
			room.BroadcastToAll(new TriggerCheckInObjectEvent_Ack(unk2: reader.ReadLEInt32(), pos: User.RoomPos, needcount: needcount, tookcount: tookcount, last: last));
		}

		public static void SubjectKing_GetQuestion(Account User, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (SubjectKingHolder.SubjectKingMapSettings.TryGetValue(room.PlayingMapNum, out var subjectnum))
			{
				List<int> questionindexs = (from s in (from w in SubjectKingHolder.SubjectKingQuestionAnswers
						where w.Value.SubjectNum == subjectnum
						select w into _
						orderby Guid.NewGuid()
						select _).Take(5)
					select s.Key).ToList();
				room.BroadcastToAll(new SubjectKing_GetQuestionAck(questionindexs, last));
			}
		}

		public static void ItemRacing_GetAbility(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int area = reader.ReadLEInt32();
			int unk = reader.ReadLEInt32();
			int length = reader.ReadLEInt32();
			byte[] buffer = reader.ReadByteArray(length);
			int mapid = room.PlayingMapNum;
			if (ItemRacingHolder.ItemRacingMapSettings.Any((KeyValuePair<int, int[]> a) => a.Key == mapid))
			{
				int group = ItemRacingHolder.ItemRacingMapSettings[mapid].OrderBy((int _) => Guid.NewGuid()).Take(1).FirstOrDefault();
				room.BroadcastToAll(new ItemRacing_GetAbilityAck(User, area, group, unk, buffer, last));
			}
		}

		public static void ItemRacing_UseAbility(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int group = reader.ReadLEInt32();
			int ability = reader.ReadLEInt32();
			int length = reader.ReadLEInt32();
			byte[] buffer = reader.ReadByteArray(length);
			room.BroadcastToAll(new ItemRacing_UseAbility(User, group, ability, buffer, last));
			if (ItemRacingHolder.ItemRacingGroupAbilities.FindAll((ItemRacingGroupAbility f) => f.GroupNum == group).Exists((ItemRacingGroupAbility c) => c.AbilityNum == 21))
			{
				Account user = (from _ in room.PlayerList()
					orderby Guid.NewGuid()
					select _).Take(1).FirstOrDefault();
				room.GameOver(user, 2, last);
			}
		}

		public static void ItemRacing_RemoveAbility(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int group = reader.ReadLEInt32();
			room.BroadcastToAll(new ItemRacing_RemoveAbility(User, group, last));
		}

		public static void TypingRun_ReqText(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room.IsTypingRunMode)
			{
				int area = User.TypingRunIndex + 1;
				int typingRunIndex = User.TypingRunIndex;
				string text = room.TypingRunText[typingRunIndex].Text;
				User.SendAsync(new TypingRun_ReqText(typingRunIndex, area, text, last));
				User.TypingRunIndex++;
			}
		}

		public static void TypingRun_EnterText(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int area = User.TypingRunIndex + 1;
			int typingRunIndex = User.TypingRunIndex;
			reader.Offset += 8;
			int time = reader.ReadLEInt32();
			if (room.IsTypingRunMode)
			{
				room.TypingRunStats.Add(new TypingRunStat
				{
					Area = area,
					EnterTime = time,
					Point = 0
				});
				int count = room.TypingRunStats.Count((TypingRunStat c) => c.Area == area);
				int rank = Array.FindIndex((from w in room.TypingRunStats
					where w.Area == area
					select w into o
					orderby o.EnterTime
					select o).ToArray(), (TypingRunStat f) => f.EnterTime == time) + 1;
				User.SendAsync(new TypingRun_EnterText(typingRunIndex, rank, count, time, last));
			}
		}

		public static void GetBonusStage_RewardList(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room.BonusStageLevel > 0)
			{
				room.BonusStageRewardGen();
				room.BroadcastToAll(new BonusStage_RewardList(room, last));
			}
		}

		public static void GetBonusStage_ExtraPoint(Account User, PacketReader reader, byte last)
		{
			int num = (User.BonusStageExtraPoint = new Random(Guid.NewGuid().GetHashCode()).Next(-99, 100));
			User.BonusStagePoint += num;
			User.SendAsync(new BonusStage_ExtraPoint(num, last));
		}

		public static void IceFlower_GetQuestion(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			int mapnum = room.PlayingMapNum;
			if (IceFlowerHolder.IceFlowerTexts.Exists((IceFlowerText e) => e.MapNum == mapnum))
			{
				List<IceFlowerText> texts = (from o in IceFlowerHolder.IceFlowerTexts.FindAll((IceFlowerText f) => f.MapNum == mapnum)
					orderby o.Index
					select o).ToList();
				float end_timing = ((num % 2 != 0) ? 1f : 1.5f);
				bool isGameOver = new Random(Guid.NewGuid().GetHashCode()).Next(2) == 1;
				room.BroadcastToAll(new IceFlower_GetQuestion(num, texts, end_timing, isGameOver, last));
			}
		}

		public static void TypingRun_GoblinRacingQuestion(Account User, PacketReader reader, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId);
			int index = reader.ReadLEInt32();
			User.TypingRunIndex++;
			User.SendAsync(new TypingRun_GoblinRacingQuestion(index, last));
		}

		public static void TypingRun_GoblinRacingTypingDone(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int index = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int msec = reader.ReadLEInt32();
			int area = User.TypingRunIndex + 1;
			room.TypingRunStats.Add(new TypingRunStat
			{
				Area = area,
				Point = ((num != 0) ? num : int.MaxValue),
				EnterTime = ((msec != 0) ? msec : int.MaxValue)
			});
			int total_user = room.TypingRunStats.Count((TypingRunStat c) => c.Area == area);
			int rank = Array.FindIndex((from w in room.TypingRunStats
				where w.Area == area
				select w into o
				orderby o.Point descending, o.EnterTime
				select o).ToArray(), (TypingRunStat f) => f.EnterTime == msec) + 1;
			User.SendAsync(new TypingRun_GoblinRacingTypingDone(index, rank, total_user, num, msec, last));
		}

		public static void Bomb_CountingStart(Account User, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId)?.BombCountingStart(User, last);
		}

		public static void Bomb_RaceTransferBomb(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int to_user = reader.ReadLEInt32();
			room?.RaceTransferBomb(User, to_user, last);
		}

		public static void UpdateUserPoint(int UserNum, int rewardGroup, int deltaPoint)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_updateUserPoint";
			mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
			mySqlCommand.Parameters.Add("rewardGroup", MySqlDbType.Int32).Value = rewardGroup;
			mySqlCommand.Parameters.Add("deltaPoint", MySqlDbType.Int32).Value = deltaPoint;
			mySqlCommand.Parameters.Add("isTodayMaxPoint", MySqlDbType.Byte).Value = 0;
			mySqlCommand.Parameters.Add("resultpoint", MySqlDbType.Int32);
			mySqlCommand.Parameters["resultpoint"].Direction = ParameterDirection.Output;
			mySqlCommand.ExecuteNonQuery();
		}

		private static byte[] GenResult_ForMiniGameMode(NormalRoom room, byte last)
		{
			PacketWriter packetWriter = new PacketWriter();
			packetWriter = PacketWriter.CreateInstance(16, LittleEndian: true);
			packetWriter.WriteOP(RoomOpcodes.eRoom_GAME_RESULT_RANK);
			packetWriter.Write((byte)room.RoomKindID);
			packetWriter.Fill((room.IsTeamPlay == 0) ? 11 : 10);
			if (room.IsTeamPlay != 0)
			{
				int team = room.Players.Values.OrderBy((Account p) => p.Rank).FirstOrDefault().Team;
				int value = ((team != 1) ? 1 : 2);
				packetWriter.Write((byte)1);
				packetWriter.Write(2);
				packetWriter.Write(team);
				packetWriter.Write(value);
				packetWriter.Write(team);
			}
			int count = room.DropItem.Count;
			packetWriter.Write((byte)count);
			foreach (DropList item in from o in room.DropItem.Values
				orderby o.Rank, o.Pos
				select o)
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
				packetWriter.Write(0L);
				if (room.IsTeamPlay != 0)
				{
					packetWriter.Write((int)item.Team);
				}
				packetWriter.Write(0);
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
				packetWriter.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B");
			}
			packetWriter.Fill(6);
			packetWriter.Write((short)0);
			packetWriter.Write(3);
			packetWriter.Write(3);
			packetWriter.Write(12 + count * 5);
			foreach (DropList item3 in room.DropItem.Values.OrderBy((DropList o) => o.Pos))
			{
				packetWriter.Write(item3.Pos);
				packetWriter.Write(item3.MiniGamePoint);
			}
			packetWriter.Write(count);
			packetWriter.Write(1);
			packetWriter.Write(4);
			byte[] nickname;
			bool flag = GameRoomEvent.SendRegisterGoods(room, out nickname, last);
			if (flag)
			{
				Account account = room.Players.Values.FirstOrDefault((Account p) => p.RoomPos == room.RoomMasterIndex);
				account.SendAsync(new GameRoom_DeleteKeepItem(account, room, last));
			}
			packetWriter.Write(1002);
			packetWriter.Write((!flag) ? 17 : (17 + nickname.Length));
			packetWriter.Write(room.ItemNum);
			if (!flag)
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
			packetWriter.Write(12);
			packetWriter.Write(last);
			byte[] result = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			packetWriter = null;
			return result;
		}
	}
}
