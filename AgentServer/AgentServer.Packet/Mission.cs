using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Mission;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class Mission
	{
		public static void Handle_GetUserMissionInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			mission_GetMissionUserInfo(currentAccount, GetLevelGroup(currentAccount.Level));
			Client.SendAsync(new GetUserMissionInfo_ACK(currentAccount, last));
		}

		public static void Handle_GetUserDailyMissionInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				reader.ReadLEInt32();
			}
			Client.SendAsync(new GetUserDailyMission_ACK(currentAccount, last));
		}

		public static void Handle_GetDailyMissionFinishedInfo(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			oneDayMission_FinishCount(currentAccount);
			Client.SendAsync(new DailyMissionFinishedInfo_ACK(3, currentAccount.DailyMission.FinishedInfo, last));
		}

		public static void Handle_AddChallengingMission(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int missionNum = reader.ReadLEInt32();
			if (mission_AddUserMissionList(currentAccount, num, missionNum, out var challengeExpireTime))
			{
				Client.SendAsync(new AddChallengingMission_ACK(num, missionNum, challengeExpireTime, last));
			}
		}

		public static void Handle_RemoveChallengingMission(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int missionNum = reader.ReadLEInt32();
			if (mission_RemoveUserMissionList(currentAccount, num, missionNum))
			{
				Client.SendAsync(new RemoveChallengingMission_ACK(num, missionNum, last));
			}
		}

		public static void Handle_UpdateMission(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int kind = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			bool flag = true;
			for (int i = 0; i < num2; i++)
			{
				int ConditionNum = reader.ReadLEInt32();
				int value = reader.ReadLEInt32();
				dictionary.Add(ConditionNum, value);
				if (MissionHolder.ConditionGroupInfo.TryGetValue(num, out var value2) && !MissionHolder.ConditionInfo.TryGetValue(value2, out var value3) && !value3.Exists((MissionConditionData e) => e.conditionNum == ConditionNum))
				{
					flag = false;
				}
				if (!flag)
				{
					break;
				}
			}
			if (flag && MissionHolder.UpdateByCondition(currentAccount, kind, num, dictionary, out var mcinfo))
			{
				Client.SendAsync(new UpdateMission_ACK(mcinfo, last));
			}
		}

		public static void Handle_CompleteMission(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			short unk = reader.ReadLEInt16();
			int num3 = reader.ReadLEInt32();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			bool flag = true;
			for (int i = 0; i < num3; i++)
			{
				int ConditionNum = reader.ReadLEInt32();
				int value = reader.ReadLEInt32();
				dictionary.Add(ConditionNum, value);
				if (MissionHolder.ConditionGroupInfo.TryGetValue(num2, out var value2) && MissionHolder.ConditionInfo.TryGetValue(value2, out var value3) && !value3.Exists((MissionConditionData e) => e.conditionNum == ConditionNum))
				{
					flag = false;
				}
				if (!flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			short challengeState = 2;
			if (MissionHolder.UpdateByCondition(currentAccount, num, num2, dictionary, out var _) && MissionHolder.ConditionDBCheck(currentAccount.UserNum, num2) && mission_CompleteMission(currentAccount.UserNum, num, num2, challengeState, out var _, out var _))
			{
				if (num == 3)
				{
					currentAccount.DailyMission.FinishedInfo.MissionFinishedInfo[num]++;
					currentAccount.DailyMission.MissionInfo[num2].challengeState = 2;
				}
				else
				{
					currentAccount.Mission.MissionInfo[num2].challengeState = 2;
				}
				Client.SendAsync(new CompleteMission_ACK(num, num2, unk, last));
				if (num == 8 && MissionHolder.QuestMissionInfo_MissionNum.TryGetValue(num2, out var value4) && quest_Complete(currentAccount, value4))
				{
					Client.SendAsync(new QuestUserInfo_ACK(isUpdate: true, currentAccount, value4, last));
				}
			}
		}

		public static void Handle_MissionGiveReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int missionNum = reader.ReadLEInt32();
			short challengeState = 5;
			lock (currentAccount.missionLock)
			{
				mission_GiveRewardEx(currentAccount.UserNum, num, missionNum, (int)currentAccount.Luck, challengeState, out var MissionList, out var exinfo);
				if (MissionList.Count <= 0 || exinfo.Count <= 0)
				{
					return;
				}
				if (num == 3)
				{
					foreach (int item in MissionList)
					{
						currentAccount.DailyMission.MissionInfo[item].challengeState = 5;
					}
				}
				else
				{
					foreach (int item2 in MissionList)
					{
						currentAccount.Mission.MissionInfo[item2].challengeState = 5;
					}
				}
				currentAccount.TRNeedUpdateFromDB = true;
				currentAccount.EXPNeedUpdateFromDB = true;
				Client.SendAsync(new MissionGiveReward_ACK(num, MissionList, exinfo, last));
			}
		}

		public static void Handle_GetGuildMissionList(ClientConnection Client, PacketReader reader, byte last)
		{
			guildMission_GetUserGuildMission(Client.CurrentAccount, out var ginfo);
			Client.SendAsync(new GetGuildMissionList_ACK(ginfo, last));
		}

		public static void Handle_SetGuildMasterMission(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			List<int> list = new List<int> { 0, 0, 0 };
			for (int i = 0; i < num; i++)
			{
				list[i] = reader.ReadLEInt32();
			}
			guildMission_SetMasterMission(currentAccount, list, out var ginfo);
			Client.SendAsync(new SetGuildMasterMission_ACK(ginfo, last));
		}

		public static void Handle_UserOptionalMissionInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			optionalMission_GetUserInfo(Client.CurrentAccount, out var UserOptionalMissionInfo);
			Client.SendAsync(new UserOptionalMissionInfo_ACK(UserOptionalMissionInfo, last));
		}

		public static void Handle_CollectionMissionInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (MissionHolder.CollectionMissionInfo.ContainsKey(num))
			{
				Client.SendAsync(new CollectionMissionInfo_ACK(num, last));
				return;
			}
			Client.SendAsync(new CollectionMissionInfo_ACK(num, last));
			Log.Debug("Unknown collection Mission Type: {0}", num);
		}

		public static void Handle_AcquireEmblem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int emblemNum = reader.ReadLEInt32();
			if (emblem_AcquireEmblem(currentAccount, emblemNum))
			{
				Client.SendAsync(new AcquireEmblem_ACK(emblemNum, last));
			}
		}

		public static void Handle_QuestAdd(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int rewardGroupID = reader.ReadLEInt32();
			if (MissionHolder.QuestMissionInfo_QuestNum.ContainsKey(num) && quest_Add(currentAccount, num, rewardGroupID, out var MissionNum))
			{
				Client.SendAsync(new AddChallengingMission_ACK(currentAccount.Mission.MissionInfo[MissionNum].MissionKind, MissionNum, 0L, last));
				Client.SendAsync(new QuestAdd_ACK(num, last));
			}
		}

		public static void Handle_QuestRemove(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (MissionHolder.QuestMissionInfo_QuestNum.ContainsKey(num) && quest_Remove(currentAccount, num, out var MissionNum))
			{
				Client.SendAsync(new RemoveChallengingMission_ACK(8, MissionNum, last));
				Client.SendAsync(new QuestRemove_ACK(num, last));
			}
		}

		public static void Handle_QuestReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int choiceRewardGroupID = reader.ReadLEInt32();
			if (MissionHolder.QuestMissionInfo_QuestNum.ContainsKey(num))
			{
				quest_Reward(currentAccount, num, choiceRewardGroupID, out var exinfo);
				if (exinfo.Count > 0)
				{
					currentAccount.TRNeedUpdateFromDB = true;
					currentAccount.EXPNeedUpdateFromDB = true;
					Client.SendAsync(new QuestReward_ACK(num, exinfo, last));
					Client.SendAsync(new QuestUserInfo_ACK(isUpdate: true, currentAccount, num, last));
				}
			}
		}

		public static int GetLevelGroup(int lv)
		{
			return (from w in new List<int>
				{
					0, 8, 15, 22, 29, 36, 43, 50, 57, 64,
					71, 78, 85, 92, 99, 106
				}
				where w <= lv
				select w into o
				orderby o descending
				select o).FirstOrDefault();
		}

		private static void oneDayMission_FinishCount(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_oneDayMission_FinishCount";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int num = 0;
					int num2 = 0;
					num = mySqlDataReader.GetInt32("missionKind");
					num2 = mySqlDataReader.GetInt32("finishCount");
					User.DailyMission.FinishedInfo.MissionFinishedInfo[num] = num2;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_oneDayMission_FinishCount Error:{0}", ex.Message);
			}
		}

		private static bool mission_AddUserMissionList(Account User, int missionKind, int missionNum, out long challengeExpireTime)
		{
			challengeExpireTime = 1842465389770955L;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr + "convert zero datetime=True;");
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_AddUserMissionList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = User.GuildNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = missionKind;
				mySqlCommand.Parameters.Add("missionNum", MySqlDbType.Int32).Value = missionNum;
				mySqlCommand.Parameters.Add("isout", MySqlDbType.Byte).Value = true;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						if (missionKind != 9 && DateTime.TryParse(mySqlDataReader["challengeExpireTime"].ToString(), out var result))
						{
							challengeExpireTime = Utility.ConvertToTimestamp(result);
						}
						MissionInfo info = new MissionInfo
						{
							MissionNum = missionNum,
							MissionKind = missionKind,
							challengeState = 0,
							challengeExpireTime = challengeExpireTime
						};
						User.Mission.MissionInfo.AddOrUpdate(missionNum, info, delegate(int k, MissionInfo v)
						{
							v = info;
							return v;
						});
						if (MissionHolder.ConditionGroupInfo.TryGetValue(missionNum, out var value) && MissionHolder.ConditionInfo.TryGetValue(value, out var value2))
						{
							foreach (MissionConditionData item in value2.Where((MissionConditionData w) => w.accumulative))
							{
								MissionConditionInfo value3 = new MissionConditionInfo
								{
									missionNum = missionNum,
									conditionNum = item.conditionNum,
									achievedPoint = 0
								};
								if (User.Mission.MissionInfo.ContainsKey(missionNum))
								{
									User.Mission.MissionInfo[missionNum].ConditionInfo.Add(item.conditionNum, value3);
								}
							}
						}
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_AddUserMissionList Error:{0}", ex.Message);
			}
			return false;
		}

		public static void mission_AddUserOneDayMissionList(Account User, int missionKind, int userLevel)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_AddUserOneDayMissionList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("userLevel", MySqlDbType.Int32).Value = userLevel;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = missionKind;
				mySqlCommand.Parameters.Add("defaultMissionCount", MySqlDbType.Int32).Value = 10;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					long challengeExpireTime = 1842465389770955L;
					if (DateTime.TryParse(mySqlDataReader["challengeExpireTime"].ToString(), out var result))
					{
						challengeExpireTime = Utility.ConvertToTimestamp(result);
					}
					MissionInfo info = new MissionInfo
					{
						MissionNum = @int,
						MissionKind = mySqlDataReader.GetInt32("kind"),
						challengeState = mySqlDataReader.GetInt32("challengeState"),
						challengeExpireTime = challengeExpireTime
					};
					User.DailyMission.MissionInfo.AddOrUpdate(@int, info, delegate(int k, MissionInfo v)
					{
						v = info;
						return v;
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_AddUserOneDayMissionList Error:{0}", ex.Message);
			}
		}

		private static void mission_GetMissionUserInfo(Account User, int userLevel)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr + "convert zero datetime=True;");
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_GetMissionUserInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("userLevel", MySqlDbType.Int32).Value = userLevel;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					long challengeExpireTime = 1842465389770955L;
					if (DateTime.TryParse(mySqlDataReader["challengeExpireTime"].ToString(), out var result) && result != DateTime.MinValue)
					{
						challengeExpireTime = Utility.ConvertToTimestamp(result);
					}
					MissionInfo info = new MissionInfo
					{
						MissionNum = @int,
						MissionKind = mySqlDataReader.GetInt32("kind"),
						challengeState = mySqlDataReader.GetInt32("challengeState"),
						challengeExpireTime = challengeExpireTime
					};
					if (info.MissionKind == 3)
					{
						User.DailyMission.MissionInfo.AddOrUpdate(@int, info, delegate(int k, MissionInfo v)
						{
							v = info;
							return v;
						});
					}
					else if (info.MissionKind != 9)
					{
						User.Mission.MissionInfo.AddOrUpdate(@int, info, delegate(int k, MissionInfo v)
						{
							v = info;
							return v;
						});
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_GetMissionUserInfo Error:{0}", ex.Message);
			}
			bool flag = User.DailyMission.MissionInfo.Count((KeyValuePair<int, MissionInfo> c) => c.Value.MissionKind == 3) == 0;
			if (!MissionHolder.MissionReloading && flag && DateTime.Now.Date == MissionHolder.EndReloadTime.Date)
			{
				mission_AddUserOneDayMissionList(User, 3, userLevel);
			}
			mission_GetMissionUserConditionInfo(User);
		}

		private static void mission_GetMissionUserConditionInfo(Account User)
		{
			foreach (KeyValuePair<int, MissionInfo> item in User.DailyMission.MissionInfo)
			{
				if (!MissionHolder.ConditionGroupInfo.TryGetValue(item.Key, out var value) || !MissionHolder.ConditionInfo.TryGetValue(value, out var value2))
				{
					continue;
				}
				foreach (MissionConditionData item2 in value2.Where((MissionConditionData w) => w.accumulative))
				{
					MissionConditionInfo value3 = new MissionConditionInfo
					{
						missionNum = item.Key,
						conditionNum = item2.conditionNum,
						achievedPoint = 0
					};
					User.DailyMission.MissionInfo[item.Key].ConditionInfo.Add(item2.conditionNum, value3);
				}
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_GetMissionUserConditionInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					int int2 = mySqlDataReader.GetInt32("conditionNum");
					int int3 = mySqlDataReader.GetInt32("achievedPoint");
					MissionConditionInfo missionConditionInfo = new MissionConditionInfo
					{
						missionNum = @int,
						conditionNum = int2,
						achievedPoint = int3
					};
					if (User.DailyMission.MissionInfo.ContainsKey(@int))
					{
						if (!User.DailyMission.MissionInfo[@int].ConditionInfo.ContainsKey(int2))
						{
							User.DailyMission.MissionInfo[@int].ConditionInfo.Add(int2, missionConditionInfo);
						}
						else
						{
							User.DailyMission.MissionInfo[@int].ConditionInfo[int2].achievedPoint = missionConditionInfo.achievedPoint;
						}
					}
					if (User.Mission.MissionInfo.ContainsKey(@int) && !User.Mission.MissionInfo[@int].ConditionInfo.ContainsKey(int2))
					{
						User.Mission.MissionInfo[@int].ConditionInfo.Add(int2, missionConditionInfo);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_GetMissionUserConditionInfo Error:{0}", ex.ToString());
			}
		}

		public static void mission_ConditionUpdateAchievedPoints(int UserNum, string strMissionNumList, string strConditionNumList, string strValueList, out List<MissionConditionInfo> mcinfo)
		{
			mcinfo = new List<MissionConditionInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_ConditionUpdateAchievedPoints";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("strMissionNumList", MySqlDbType.VarString).Value = strMissionNumList;
				mySqlCommand.Parameters.Add("strConditionNumList", MySqlDbType.VarString).Value = strConditionNumList;
				mySqlCommand.Parameters.Add("strValueList", MySqlDbType.VarString).Value = strValueList;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					int int2 = mySqlDataReader.GetInt32("conditionNum");
					int int3 = mySqlDataReader.GetInt32("achievedPoint");
					MissionConditionInfo item = new MissionConditionInfo
					{
						missionNum = @int,
						conditionNum = int2,
						achievedPoint = int3
					};
					mcinfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_ConditionUpdateAchievedPoints Error:{0}", ex.Message);
			}
		}

		public static void mission_ConditionDBCheck(int UserNum, string strConditionNumList, string strTypeList, string strValueList, out Dictionary<int, int> mcinfo)
		{
			mcinfo = new Dictionary<int, int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_ConditionDBCheck";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("strConditionNumList", MySqlDbType.VarString).Value = strConditionNumList;
				mySqlCommand.Parameters.Add("strTypeList", MySqlDbType.VarString).Value = strTypeList;
				mySqlCommand.Parameters.Add("strValueList", MySqlDbType.VarString).Value = strValueList;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("conditionNum");
					int int2 = mySqlDataReader.GetInt32("achievedPoint");
					mcinfo.Add(@int, int2);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_ConditionDBCheck Error:{0}", ex.Message);
			}
		}

		private static bool mission_CompleteMission(int UserNum, int missionKind, int missionNum, short challengeState, out int playTime, out int completeCount)
		{
			playTime = 0;
			completeCount = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_CompleteMission";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = missionKind;
				mySqlCommand.Parameters.Add("missionNum", MySqlDbType.Int32).Value = missionNum;
				mySqlCommand.Parameters.Add("challengeState", MySqlDbType.Int16).Value = challengeState;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					playTime = Convert.ToInt32(mySqlDataReader["playTime"]);
					completeCount = mySqlDataReader.GetInt32("completeCount");
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_CompleteMission Error:{0}", ex.Message);
			}
			return false;
		}

		private static void mission_GiveRewardEx(int UserNum, int missionKind, int missionNum, int lucky, short challengeState, out List<int> MissionList, out List<ExchangeItemInfo> exinfo)
		{
			MissionList = new List<int>();
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_GiveRewardEx";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = missionKind;
				mySqlCommand.Parameters.Add("missionNum", MySqlDbType.Int32).Value = missionNum;
				mySqlCommand.Parameters.Add("pLucky", MySqlDbType.Int32).Value = lucky;
				mySqlCommand.Parameters.Add("challengeState", MySqlDbType.Int32).Value = challengeState;
				mySqlCommand.Parameters.Add("strengthenItemNum", MySqlDbType.Int32).Value = -1;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					MissionList.Add(mySqlDataReader.GetInt32("MissionNum"));
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					ExchangeItemInfo item = new ExchangeItemInfo
					{
						type = Convert.ToInt32(mySqlDataReader["rewardType"]),
						id = Convert.ToInt32(mySqlDataReader["rewardID"]),
						count = Convert.ToInt32(mySqlDataReader["amount"])
					};
					Convert.ToInt32(mySqlDataReader["playTime"]);
					exinfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_GiveRewardEx Error:{0}", ex.Message);
			}
		}

		private static bool mission_RemoveUserMissionList(Account User, int missionKind, int missionNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_mission_RemoveUserMissionList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = missionKind;
				mySqlCommand.Parameters.Add("missionNum", MySqlDbType.Int32).Value = missionNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						User.Mission.MissionInfo.TryRemove(missionNum, out var _);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_mission_RemoveUserMissionList Error:{0}", ex.Message);
			}
			return false;
		}

		private static void guildMission_GetUserGuildMission(Account User, out List<GuildMissionInfo> ginfo)
		{
			ginfo = new List<GuildMissionInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMission_GetUserGuildMission";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = User.GuildNum;
				mySqlCommand.Parameters.Add("defaultMissionCount", MySqlDbType.Int32).Value = 1;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildMissionInfo item = new GuildMissionInfo
					{
						missionNum = mySqlDataReader.GetInt32("missionNum"),
						isMaster = mySqlDataReader.GetBoolean("masterMission")
					};
					ginfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMission_GetUserGuildMission Error:{0}", ex.Message);
			}
		}

		private static void guildMission_SetMasterMission(Account User, List<int> iinfo, out List<int> ginfo)
		{
			ginfo = new List<int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMission_SetMasterMission";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = User.GuildNum;
				mySqlCommand.Parameters.Add("missionNum1", MySqlDbType.Int32).Value = iinfo[0];
				mySqlCommand.Parameters.Add("missionNum2", MySqlDbType.Int32).Value = iinfo[1];
				mySqlCommand.Parameters.Add("missionNum3", MySqlDbType.Int32).Value = iinfo[2];
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ginfo.Add(mySqlDataReader.GetInt32("missionNum"));
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMission_SetMasterMission Error:{0}", ex.Message);
			}
		}

		private static bool optionalMission_GetUserInfo(Account User, out Dictionary<int, int> UserOptionalMissionInfo)
		{
			UserOptionalMissionInfo = new Dictionary<int, int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_optionalMission_GetUserInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					int int2 = mySqlDataReader.GetInt32("completeCount");
					if (!UserOptionalMissionInfo.ContainsKey(@int))
					{
						UserOptionalMissionInfo.Add(@int, int2);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_optionalMission_GetUserInfo Error:{0}", ex.Message);
			}
			return false;
		}

		private static bool emblem_AcquireEmblem(Account User, int emblemNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_emblem_AcquireEmblem";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("emblemNum", MySqlDbType.Int32).Value = emblemNum;
				mySqlCommand.Parameters.Add("rankResult", MySqlDbType.Byte).Value = true;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_emblem_AcquireEmblem Error:{0}", ex.Message);
			}
			return false;
		}

		public static void quest_UserQuestInfo(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_quest_UserQuestInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					QuestInfo questInfo = new QuestInfo
					{
						questNum = mySqlDataReader.GetInt32("questNum"),
						challengeState = mySqlDataReader.GetInt32("challengeState"),
						completeCount = mySqlDataReader.GetInt32("completeCount"),
						startTime = ((mySqlDataReader.GetString("startTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("startTime"))),
						endTime = ((mySqlDataReader.GetString("endTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("endTime")))
					};
					User.QuestInfo.TryAdd(questInfo.questNum, questInfo);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_quest_UserQuestInfo Error:{0} UserNum:{1}", ex.Message, User.UserNum);
			}
		}

		private static bool quest_Add(Account User, int questNum, int rewardGroupID, out int MissionNum)
		{
			MissionNum = 0;
			if (!MissionHolder.QuestMissionInfo_QuestNum.TryGetValue(questNum, out MissionNum))
			{
				return false;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_quest_Add";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("questNum", MySqlDbType.Int32).Value = questNum;
				mySqlCommand.Parameters.Add("rewardGroupID", MySqlDbType.Int32).Value = rewardGroupID;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					MissionInfo info = new MissionInfo
					{
						MissionNum = MissionNum,
						MissionKind = 8,
						challengeState = 0,
						challengeExpireTime = 0L
					};
					User.Mission.MissionInfo.AddOrUpdate(info.MissionNum, info, delegate(int k, MissionInfo v)
					{
						v = info;
						return v;
					});
					if (MissionHolder.ConditionGroupInfo.TryGetValue(info.MissionNum, out var value) && MissionHolder.ConditionInfo.TryGetValue(value, out var value2))
					{
						foreach (MissionConditionData item in value2.Where((MissionConditionData w) => w.accumulative))
						{
							MissionConditionInfo value3 = new MissionConditionInfo
							{
								missionNum = info.MissionNum,
								conditionNum = item.conditionNum,
								achievedPoint = 0
							};
							if (User.Mission.MissionInfo.ContainsKey(info.MissionNum))
							{
								User.Mission.MissionInfo[info.MissionNum].ConditionInfo.Add(item.conditionNum, value3);
							}
						}
					}
					QuestInfo qinfo = new QuestInfo
					{
						questNum = questNum,
						challengeState = 0,
						completeCount = 0,
						startTime = Utility.ConvertToTimestamp(DateTime.Now),
						endTime = 1842465389770955L
					};
					User.QuestInfo.AddOrUpdate(qinfo.questNum, qinfo, delegate(int k, QuestInfo v)
					{
						v = qinfo;
						return v;
					});
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_quest_Add questNum:{1}, Error:{0}", ex.Message, questNum);
			}
			return false;
		}

		private static bool quest_Remove(Account User, int questNum, out int MissionNum)
		{
			MissionNum = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_quest_Remove";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("questNum", MySqlDbType.Int32).Value = questNum;
				mySqlCommand.Parameters.Add("pout", MySqlDbType.Int32).Value = 1;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					User.QuestInfo.TryRemove(questNum, out var _);
					if (MissionHolder.QuestMissionInfo_QuestNum.TryGetValue(questNum, out MissionNum))
					{
						User.Mission.MissionInfo.TryRemove(MissionNum, out var _);
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_quest_Remove questNum:{1}, Error:{0}", ex.Message, questNum);
			}
			return false;
		}

		private static bool quest_Complete(Account User, int questNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_quest_Complete";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("questNum", MySqlDbType.Int32).Value = questNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					User.QuestInfo[questNum].challengeState = 2;
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_quest_Complete questNum:{1}, Error:{0}", ex.Message, questNum);
			}
			return false;
		}

		private static void quest_Reward(Account User, int questNum, int choiceRewardGroupID, out List<ExchangeItemInfo> exinfo)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_quest_Reward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("questNum", MySqlDbType.Int32).Value = questNum;
				mySqlCommand.Parameters.Add("choiceRewardGroupID", MySqlDbType.Int32).Value = choiceRewardGroupID;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				int num = 0;
				while (mySqlDataReader.Read())
				{
					ExchangeItemInfo item = new ExchangeItemInfo
					{
						type = Convert.ToInt32(mySqlDataReader["rewardType"]),
						id = Convert.ToInt32(mySqlDataReader["rewardItem"]),
						count = Convert.ToInt32(mySqlDataReader["rewardCount"])
					};
					num = Convert.ToInt32(mySqlDataReader["missionNum"]);
					exinfo.Add(item);
				}
				User.QuestInfo[questNum].challengeState = 3;
				User.QuestInfo[questNum].completeCount++;
				User.QuestInfo[questNum].endTime = Utility.ConvertToTimestamp(DateTime.Now);
				if (num > 0)
				{
					User.Mission.MissionInfo[num].challengeState = 2;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_quest_Reward questNum:{1}, Error:{0}", ex.Message, questNum);
			}
		}
	}
}
