using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using AgentServer.Structuring.TalesKnight;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using Serilog;
using Weighted_Randomizer;

namespace AgentServer.Holders
{
	public static class TalesKnightHolder
	{
		public static ConcurrentDictionary<int, TalesKnightStageRewardInfo> TalesKnightStageReward { get; set; } = new ConcurrentDictionary<int, TalesKnightStageRewardInfo>();


		public static NestedDictionary<int, int, IWeightedRandomizer<TalesKnightStageRewardDetailInfo>> TalesKnightStageRewardDetail { get; set; } = new NestedDictionary<int, int, IWeightedRandomizer<TalesKnightStageRewardDetailInfo>>();


		public static void LoadTalesKnightStageReward()
		{
			TalesKnightStageReward.Clear();
			NestedDictionary<int, int, List<TalesKnightStageRewardDetailInfo>> nestedDictionary = new NestedDictionary<int, int, List<TalesKnightStageRewardDetailInfo>>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getTalesKnightsRewardInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("StageGroupNum");
					TalesKnightStageRewardInfo value = new TalesKnightStageRewardInfo
					{
						StageGroupNum = @int,
						RewardGroupNum = mySqlDataReader.GetInt32("RewardGroupNum"),
						VALUEMAX = mySqlDataReader.GetInt32("VALUEMAX"),
						VALUEMIN = mySqlDataReader.GetInt32("VALUEMIN"),
						RewardType = mySqlDataReader.GetInt32("RewardType")
					};
					TalesKnightStageReward.TryAdd(@int, value);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					int int2 = mySqlDataReader.GetInt32("RewardStageGroupNum");
					int int3 = mySqlDataReader.GetInt32("RewardGroupNum");
					TalesKnightStageRewardDetailInfo item = new TalesKnightStageRewardDetailInfo
					{
						RewardStageGroupNum = int2,
						RewardGroupNum = int3,
						RewardItemNum = mySqlDataReader.GetInt32("RewardItemNum"),
						RewardCount = mySqlDataReader.GetInt32("RewardCount"),
						HolderUnit = mySqlDataReader.GetInt32("HolderUnit")
					};
					if (!nestedDictionary.ContainsKey(int2, int3))
					{
						nestedDictionary.Add(int2, int3, new List<TalesKnightStageRewardDetailInfo> { item });
					}
					else
					{
						nestedDictionary[int2][int3].Add(item);
					}
				}
			}
			foreach (KeyValuePair<int, NestedDictionary<int, List<TalesKnightStageRewardDetailInfo>>> item2 in nestedDictionary)
			{
				foreach (KeyValuePair<int, List<TalesKnightStageRewardDetailInfo>> item3 in item2.Value)
				{
					StaticWeightedRandomizer<TalesKnightStageRewardDetailInfo> staticWeightedRandomizer = new StaticWeightedRandomizer<TalesKnightStageRewardDetailInfo>();
					foreach (TalesKnightStageRewardDetailInfo item4 in item3.Value)
					{
						staticWeightedRandomizer.Add(item4, item4.RewardCount);
					}
					TalesKnightStageRewardDetail[item2.Key][item3.Key] = staticWeightedRandomizer;
				}
			}
			Log.Information("Load TalesKnightStageReward Done!");
		}
	}
}
