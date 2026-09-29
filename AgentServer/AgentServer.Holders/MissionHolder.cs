using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Packet;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using MySql.Data.MySqlClient;
using Quartz;
using Serilog;

namespace AgentServer.Holders
{
	public static class MissionHolder
	{
		public static ConcurrentDictionary<int, int> ConditionGroupInfo = new ConcurrentDictionary<int, int>();

		public static ConcurrentDictionary<int, List<MissionConditionData>> ConditionInfo = new ConcurrentDictionary<int, List<MissionConditionData>>();

		public static ConcurrentDictionary<int, MissionConditionData> ConditionInfoByConditionNum = new ConcurrentDictionary<int, MissionConditionData>();

		public static ConcurrentDictionary<int, List<int>> CollectionMissionInfo = new ConcurrentDictionary<int, List<int>>();

		public static ConcurrentDictionary<int, List<MissionInfo>> DailyMissionInfo = new ConcurrentDictionary<int, List<MissionInfo>>();

		public static ConcurrentDictionary<int, int> QuestMissionInfo_QuestNum = new ConcurrentDictionary<int, int>();

		public static ConcurrentDictionary<int, int> QuestMissionInfo_MissionNum = new ConcurrentDictionary<int, int>();

		public static HashSet<int> QuestEventInfo = new HashSet<int>();

		public static DateTime EndReloadTime = DateTime.Now;

		private static IActorRef MissionActor;

		private static bool _MissionReloading = false;

		private static bool _UserMissionReloading = false;

		private static object sync = new object();

		public static bool MissionReloading
		{
			get
			{
				lock (sync)
				{
					return _MissionReloading;
				}
			}
			set
			{
				lock (sync)
				{
					_MissionReloading = value;
				}
			}
		}

		public static bool UserMissionReloading
		{
			get
			{
				lock (sync)
				{
					return _UserMissionReloading;
				}
			}
			set
			{
				lock (sync)
				{
					_UserMissionReloading = value;
				}
			}
		}

		public static void LoadMissionDataInfo()
		{
			try
			{
				LoadSchedule();
				Mission_LoadConditionGroupInfo();
				Mission_LoadConditionInfo();
				Mission_LoadCollectionMissionInfo();
				Mission_LoadToDayMissionInfo();
				Mission_LoadQuestMissionInfo();
				Mission_LoadQuestEventInfo();
				Log.Information("Load MissionDataInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("LoadMissionDataInfo Error:{0}", ex.Message);
			}
		}

		public static bool ConditionDBCheck(int UserNum, int missionNum)
		{
			if (ConditionGroupInfo.TryGetValue(missionNum, out var value) && ConditionInfo.TryGetValue(value, out var value2))
			{
				string text = string.Empty;
				string text2 = string.Empty;
				string text3 = string.Empty;
				foreach (MissionConditionData item in value2)
				{
					switch (item.type)
					{
					case 12:
					case 22:
					case 26:
					case 28:
					case 37:
					case 51:
					case 75:
					case 78:
					case 94:
					case 113:
						text2 += $"{item.conditionNum},";
						text += $"{item.type},";
						text3 += $"{item.goalPoint},";
						break;
					}
				}
				if (!string.IsNullOrEmpty(text2))
				{
					Mission.mission_ConditionDBCheck(UserNum, text2, text, text3, out var mcinfo);
					foreach (MissionConditionData item2 in value2)
					{
						switch (item2.type)
						{
						case 12:
						case 22:
						case 26:
						case 28:
						case 37:
						case 51:
						case 75:
						case 78:
						case 94:
						case 113:
							if (item2.goalPoint < mcinfo[item2.conditionNum])
							{
								return false;
							}
							break;
						}
					}
				}
			}
			return true;
		}

		public static bool UpdateByCondition(Account User, int kind, int missionNum, Dictionary<int, int> ConditionInfo, out List<MissionConditionInfo> mcinfo)
		{
			switch (kind)
			{
			case 1:
			case 8:
			{
				MissionInfo value;
				bool flag = User.Mission.MissionInfo.TryGetValue(missionNum, out value);
				if (flag && value.ConditionInfo.Count == 0)
				{
					mcinfo = null;
					return true;
				}
				if (!flag)
				{
					mcinfo = null;
					return false;
				}
				string text4 = string.Empty;
				string text5 = string.Empty;
				string text6 = string.Empty;
				foreach (KeyValuePair<int, int> item in ConditionInfo)
				{
					if (value.ConditionInfo.ContainsKey(item.Key) && ConditionInfoByConditionNum.TryGetValue(item.Key, out var value2) && value2.operatorType == 2)
					{
						int achievedPoint = value.ConditionInfo[item.Key].achievedPoint;
						switch (value2.type)
						{
						case 8:
						case 18:
							achievedPoint++;
							break;
						case 71:
							achievedPoint += item.Value;
							break;
						default:
							achievedPoint++;
							break;
						}
						text4 += $"{missionNum},";
						text5 += $"{item.Key},";
						text6 += $"{achievedPoint},";
					}
				}
				if (string.IsNullOrEmpty(text4))
				{
					break;
				}
				Mission.mission_ConditionUpdateAchievedPoints(User.UserNum, text4, text5, text6, out mcinfo);
				if (mcinfo.Count <= 0)
				{
					break;
				}
				foreach (MissionConditionInfo item2 in mcinfo)
				{
					value.ConditionInfo[item2.conditionNum].achievedPoint = item2.achievedPoint;
				}
				return true;
			}
			case 2:
			case 3:
			{
				string text = string.Empty;
				string text2 = string.Empty;
				string text3 = string.Empty;
				if (kind == 3)
				{
					foreach (KeyValuePair<int, MissionInfo> item3 in User.DailyMission.MissionInfo.Where((KeyValuePair<int, MissionInfo> w) => w.Key == missionNum))
					{
						foreach (KeyValuePair<int, int> item4 in ConditionInfo)
						{
							if (item3.Value.ConditionInfo.ContainsKey(item4.Key))
							{
								text += $"{item3.Key},";
								text2 += $"{item4.Key},";
								text3 += $"{item4.Value},";
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<int, MissionInfo> item5 in User.Mission.MissionInfo.Where((KeyValuePair<int, MissionInfo> w) => w.Key == missionNum))
					{
						foreach (KeyValuePair<int, int> item6 in ConditionInfo)
						{
							if (item5.Value.ConditionInfo.ContainsKey(item6.Key))
							{
								text += $"{item5.Key},";
								text2 += $"{item6.Key},";
								text3 += $"{item6.Value},";
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					Mission.mission_ConditionUpdateAchievedPoints(User.UserNum, text, text2, text3, out mcinfo);
				}
				mcinfo = null;
				return true;
			}
			}
			mcinfo = null;
			return false;
		}

		public static void optional_collectionMission_Load(Account User)
		{
			foreach (int key in CollectionMissionInfo.Keys)
			{
				collectionMission_GetUserMission(User, key);
			}
		}

		private static void optionalMission_addUserMission(int UserNum, string requestMissionNumList)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_optionalMission_addUserMission";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = 9;
				mySqlCommand.Parameters.Add("requestMissionNumList", MySqlDbType.VarString).Value = requestMissionNumList;
				using (mySqlCommand.ExecuteReader())
				{
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_oneDayMission_FinishCount Error:{0}", ex.Message);
			}
		}

		private static bool collectionMission_GetUserMission(Account User, int collectionType)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_collectionMission_GetUserMission";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = 11;
				mySqlCommand.Parameters.Add("collectionType", MySqlDbType.Int32).Value = collectionType;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				long challengeExpireTime = 1842465389770955L;
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("missionNum");
					MissionInfo info = new MissionInfo
					{
						MissionNum = @int,
						MissionKind = mySqlDataReader.GetInt32("missionKind"),
						challengeState = mySqlDataReader.GetInt32("challengeState"),
						challengeExpireTime = challengeExpireTime
					};
					User.Mission.MissionInfo.AddOrUpdate(@int, info, delegate(int k, MissionInfo v)
					{
						v = info;
						return v;
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_collectionMission_GetUserMission Error:{0}", ex.Message);
			}
			return false;
		}

		private static void LoadSchedule()
		{
			MissionActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new MissionManager()), "MissionManager");
			ServerStatus.QuartzActor.Tell(new CreateJob(MissionActor, 1, TriggerBuilder.Create().WithCronSchedule("10 0 0 ? * * *").Build()));
			ServerStatus.QuartzActor.Tell(new CreateJob(MissionActor, 2, TriggerBuilder.Create().WithCronSchedule("15 0 0 ? * * *").Build()));
			ServerStatus.QuartzActor.Tell(new CreateJob(MissionActor, 3, TriggerBuilder.Create().WithCronSchedule("30 0 0 ? * * *").Build()));
		}

		private static void Mission_LoadConditionInfo()
		{
			ConditionInfo.Clear();
			ConditionInfoByConditionNum.Clear();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select fdGroupNum as 'groupNum',\r\n\t                                                       fdConditionNum as 'conditionNum',\r\n\t                                                       fdType as 'type',\r\n\t                                                       fdOperatorType as 'operatorType',\r\n\t                                                       fdGoalPoint as 'goalPoint',\r\n\t                                                       fdAccumulative as 'accumulative'\r\n                                                    from EssenMissionConditionGroup\r\n                                                    order by fdConditionNum asc", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int num = Convert.ToInt32(mySqlDataReader["conditionNum"]);
					int num2 = Convert.ToInt32(mySqlDataReader["groupNum"]);
					MissionConditionData info = new MissionConditionData
					{
						groupNum = num2,
						conditionNum = num,
						type = Convert.ToInt32(mySqlDataReader["type"]),
						operatorType = Convert.ToInt32(mySqlDataReader["operatorType"]),
						goalPoint = Convert.ToInt32(mySqlDataReader["goalPoint"]),
						accumulative = Convert.ToBoolean(mySqlDataReader["accumulative"])
					};
					ConditionInfo.AddOrUpdate(num2, new List<MissionConditionData> { info }, delegate(int k, List<MissionConditionData> v)
					{
						v.Add(info);
						return v;
					});
					ConditionInfoByConditionNum.TryAdd(num, info);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Mission_LoadConditionInfo Error:{0}", ex.Message);
			}
		}

		private static void Mission_LoadConditionGroupInfo()
		{
			ConditionGroupInfo.Clear();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select fdMissionNum as 'missionNum', \r\n\t                                                         fdGroupNum as 'groupNum'\r\n                                                    from EssenMissionConditionGroupList order by fdNum asc", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["missionNum"]);
					int value = Convert.ToInt32(mySqlDataReader["groupNum"]);
					ConditionGroupInfo.TryAdd(key, value);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Mission_LoadConditionGroupInfo Error:{0}", ex.Message);
			}
		}

		private static void Mission_LoadCollectionMissionInfo()
		{
			CollectionMissionInfo.Clear();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select fdCollectionNum as 'collectionNum', \r\n\t                                                           fdMissionNum as 'missionNum' ,\r\n\t                                                           fdType as 'type'\r\n                                                        from EssenCollectionMissionInfo\r\n                                                        order by fdCollectionNum asc, fdMissionNum asc", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["type"]);
					int missionNum = Convert.ToInt32(mySqlDataReader["missionNum"]);
					CollectionMissionInfo.AddOrUpdate(key, new List<int> { missionNum }, delegate(int k, List<int> v)
					{
						v.Add(missionNum);
						return v;
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("Mission_LoadCollectionMissionInfo Error:{0}", ex.Message);
			}
		}

		public static void Mission_LoadToDayMissionInfo(bool init = true)
		{
			DailyMissionInfo.Clear();
			bool flag = false;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_oneDayMission_GetToDayMission";
				mySqlCommand.Parameters.Add("toDayDate", MySqlDbType.DateTime).Value = DateTime.Now;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				flag = mySqlDataReader.HasRows;
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("Level");
					int int2 = mySqlDataReader.GetInt32("missionKind");
					MissionInfo info = new MissionInfo
					{
						MissionNum = mySqlDataReader.GetInt32("missionNum"),
						MissionKind = int2
					};
					if (int2 == 3)
					{
						DailyMissionInfo.AddOrUpdate(@int, new List<MissionInfo> { info }, delegate(int k, List<MissionInfo> v)
						{
							v.Add(info);
							return v;
						});
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_oneDayMission_GetToDayMission Error:{0}", ex.Message);
			}
			if (!(flag && init))
			{
				Mission_oneDayMission_Init();
			}
		}

		public static void Mission_oneDayMission_Init()
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_oneDayMission_Init";
					mySqlCommand.Parameters.Add("missionKind", MySqlDbType.Int32).Value = 3;
					mySqlCommand.Parameters.Add("defaultMissionCount", MySqlDbType.Int32).Value = 10;
					mySqlCommand.Parameters.Add("currentDate", MySqlDbType.DateTime).Value = DateTime.Now;
					mySqlCommand.ExecuteNonQuery();
				}
				Log.Information("OneDayMission Init Done!");
			}
			catch (Exception ex)
			{
				Log.Error("usp_oneDayMission_Init Error:{0}", ex.Message);
			}
		}

		public static void Mission_guildMission_Init()
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildMission_Init";
					mySqlCommand.ExecuteNonQuery();
				}
				Log.Information("GuildMission Init Done!");
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMission_Init Error:{0}", ex.Message);
			}
		}

		private static void Mission_LoadQuestMissionInfo()
		{
			QuestMissionInfo_QuestNum.Clear();
			QuestMissionInfo_MissionNum.Clear();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("select fdQuestNum as 'questNum', \r\n\t                                                           fdMissionNum as 'missionNum' \r\n                                                        from EssenQuestMissionInfo\r\n                                                        order by fdQuestNum asc, fdMissionNum asc", mySqlConnection);
					mySqlCommand.Parameters.Clear();
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int questNum = Convert.ToInt32(mySqlDataReader["questNum"]);
						int missionNum = Convert.ToInt32(mySqlDataReader["missionNum"]);
						QuestMissionInfo_QuestNum.AddOrUpdate(questNum, missionNum, delegate(int k, int v)
						{
							v = missionNum;
							return v;
						});
						QuestMissionInfo_MissionNum.AddOrUpdate(missionNum, questNum, delegate(int k, int v)
						{
							v = questNum;
							return v;
						});
					}
				}
				Log.Information("Load QuestMissionInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Mission_LoadQuestMissionInfo Error:{0}", ex.Message);
			}
		}

		private static void Mission_LoadQuestEventInfo()
		{
			QuestEventInfo.Clear();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("select fdQuestNum as 'questNum'\r\n                                                        from EssenQuestEventInfo where fdIsUse = 1", mySqlConnection);
					mySqlCommand.Parameters.Clear();
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int item = Convert.ToInt32(mySqlDataReader["questNum"]);
						QuestEventInfo.Add(item);
					}
				}
				Log.Information("Load QuestEventInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Mission_LoadQuestEventInfo Error:{0}", ex.Message);
			}
		}
	}
}
