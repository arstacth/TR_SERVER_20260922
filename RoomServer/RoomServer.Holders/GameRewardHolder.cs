using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using RoomServer.Structuring.GameReward;
using Serilog;
using Weighted_Randomizer;

namespace RoomServer.Holders
{
	public static class GameRewardHolder
	{
		public static ConcurrentDictionary<int, GameRewardGroupInfo> GroupInfos { get; set; } = new ConcurrentDictionary<int, GameRewardGroupInfo>();


		public static NestedDictionary<int, int, IWeightedRandomizer<GameRewardGroupRate>> GroupRateInfos { get; set; } = new NestedDictionary<int, int, IWeightedRandomizer<GameRewardGroupRate>>();


		public static ConcurrentDictionary<int, IWeightedRandomizer<GameRewardSubGroupInfo>> SubGroupInfos { get; set; } = new ConcurrentDictionary<int, IWeightedRandomizer<GameRewardSubGroupInfo>>();


		public static void LoadGameRewardInfo()
		{
			GroupInfos.Clear();
			GroupRateInfos.Clear();
			SubGroupInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_gameReward_GetGroupInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["groupNum"]);
					GameRewardGroupInfo value = new GameRewardGroupInfo
					{
						GroupType = Convert.ToInt16(mySqlDataReader["groupType"]),
						Argument = Convert.ToInt32(mySqlDataReader["arg"]),
						ChildGroupNum = Convert.ToInt32(mySqlDataReader["childGroupNum"]),
						SpecialRewardRate = Convert.ToSingle(mySqlDataReader["specialRewardRate"]),
						UsePeriod = Convert.ToBoolean(mySqlDataReader["usePeriod"]),
						StartTime = Convert.ToDateTime(mySqlDataReader["startTime"]),
						EndTime = Convert.ToDateTime(mySqlDataReader["endTime"]),
						StartHour = Convert.ToInt32(mySqlDataReader["startHour"]),
						EndHour = Convert.ToInt32(mySqlDataReader["endHour"])
					};
					GroupInfos.TryAdd(key, value);
				}
			}
			NestedDictionary<int, int, int, GameRewardGroupRate> nestedDictionary = new NestedDictionary<int, int, int, GameRewardGroupRate>();
			using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection2.Open();
				using MySqlCommand mySqlCommand2 = new MySqlCommand(string.Empty, mySqlConnection2);
				mySqlCommand2.Parameters.Clear();
				mySqlCommand2.CommandType = CommandType.StoredProcedure;
				mySqlCommand2.CommandText = "usp_gameReward_GetGroupRate";
				using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
				while (mySqlDataReader2.Read())
				{
					int num = Convert.ToInt32(mySqlDataReader2["groupNum"]);
					int num2 = Convert.ToInt32(mySqlDataReader2["subGroupNum"]);
					int num3 = Convert.ToInt32(mySqlDataReader2["rewardID"]);
					GameRewardGroupRate gameRewardGroupRate = new GameRewardGroupRate
					{
						GroupNum = num,
						SubGroup = num2,
						Rate = Convert.ToSingle(mySqlDataReader2["rate"]),
						RewardType = Convert.ToInt16(mySqlDataReader2["rewardType"]),
						RewardID = num3,
						Amount = Convert.ToInt32(mySqlDataReader2["amount"])
					};
					if (gameRewardGroupRate.RewardID != 46235 && (gameRewardGroupRate.RewardID < 27685 || gameRewardGroupRate.RewardID > 27709) && num >= 100)
					{
						nestedDictionary.Add(num, num2, num3, gameRewardGroupRate);
					}
				}
			}
			foreach (KeyValuePair<int, NestedDictionary<int, int, GameRewardGroupRate>> item in nestedDictionary)
			{
				foreach (KeyValuePair<int, NestedDictionary<int, GameRewardGroupRate>> item2 in item.Value)
				{
					IWeightedRandomizer<GameRewardGroupRate> weightedRandomizer = new StaticWeightedRandomizer<GameRewardGroupRate>();
					foreach (KeyValuePair<int, GameRewardGroupRate> item3 in item2.Value)
					{
						weightedRandomizer.Add(item3.Value, Convert.ToInt32(item3.Value.Rate * 1000f));
					}
					if (weightedRandomizer.Count > 0)
					{
						GroupRateInfos.Add(item.Key, item2.Key, weightedRandomizer);
					}
				}
			}
			NestedDictionary<int, int, GameRewardSubGroupInfo> nestedDictionary2 = new NestedDictionary<int, int, GameRewardSubGroupInfo>();
			using (MySqlConnection mySqlConnection3 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection3.Open();
				using MySqlCommand mySqlCommand3 = new MySqlCommand(string.Empty, mySqlConnection3);
				mySqlCommand3.Parameters.Clear();
				mySqlCommand3.CommandType = CommandType.StoredProcedure;
				mySqlCommand3.CommandText = "usp_gameReward_GetSubGroupInfo";
				using MySqlDataReader mySqlDataReader3 = mySqlCommand3.ExecuteReader();
				while (mySqlDataReader3.Read())
				{
					int key2 = Convert.ToInt32(mySqlDataReader3["groupNum"]);
					int num4 = Convert.ToInt32(mySqlDataReader3["subGroupNum"]);
					int num5 = Convert.ToInt32(mySqlDataReader3["dependentQuestNum"]);
					GameRewardSubGroupInfo value2 = new GameRewardSubGroupInfo
					{
						SubGroup = num4,
						SubGroupType = Convert.ToInt32(mySqlDataReader3["subGroupType"]),
						SubGroupRate = Convert.ToSingle(mySqlDataReader3["subGroupRate"]),
						StartRank = Convert.ToByte(mySqlDataReader3["startRank"]),
						EndRank = Convert.ToByte(mySqlDataReader3["endRank"]),
						RaceRate = Convert.ToSingle(mySqlDataReader3["raceRate"]) / 100f,
						OnePlusOneRate = Convert.ToSingle(mySqlDataReader3["onePlusOneRate"])
					};
					if (GroupRateInfos.ContainsKey(key2, num4) && num5 == 0)
					{
						nestedDictionary2.Add(key2, num4, value2);
					}
				}
			}
			foreach (KeyValuePair<int, NestedDictionary<int, GameRewardSubGroupInfo>> item4 in nestedDictionary2)
			{
				IWeightedRandomizer<GameRewardSubGroupInfo> weightedRandomizer2 = new StaticWeightedRandomizer<GameRewardSubGroupInfo>();
				foreach (KeyValuePair<int, GameRewardSubGroupInfo> item5 in item4.Value)
				{
					if (item5.Value.SubGroupType != 100 && !(item5.Value.SubGroupRate < 0f))
					{
						weightedRandomizer2.Add(item5.Value, Convert.ToInt32(item5.Value.SubGroupRate * 10f));
					}
				}
				if (weightedRandomizer2.Count > 0)
				{
					SubGroupInfos.TryAdd(item4.Key, weightedRandomizer2);
				}
			}
			Log.Information("Load GameRewardInfo Done!");
		}
	}
}
