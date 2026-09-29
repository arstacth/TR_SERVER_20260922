using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using RoomServer.Structuring.Map;
using Serilog;
using Weighted_Randomizer;

namespace RoomServer.Holders
{
	public static class MapItemHolder
	{
		public static ConcurrentDictionary<int, CapsuleItemMapJoint> CapsuleItemMapJoints { get; } = new ConcurrentDictionary<int, CapsuleItemMapJoint>();


		public static ConcurrentDictionary<int, IWeightedRandomizer<MapCapsuleItemInfo>> MapCapsuleItems { get; } = new ConcurrentDictionary<int, IWeightedRandomizer<MapCapsuleItemInfo>>();


		public static NestedDictionary<int, int, int, AssaultModeRewardInfo> AssaultModeRewardInfos { get; } = new NestedDictionary<int, int, int, AssaultModeRewardInfo>();


		public static void LoadCapsuleItemMapJoint()
		{
			CapsuleItemMapJoints.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from EssenCapsuleItemMapJoint;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					CapsuleItemMapJoints.TryAdd(key, new CapsuleItemMapJoint
					{
						GroupNum = Convert.ToInt32(mySqlDataReader["fdGroupNum"]),
						SlotCount = Convert.ToInt32(mySqlDataReader["fdSlotCount"])
					});
				}
			}
			Log.Information("Load CapsuleItemMapJoint Count: {0}", CapsuleItemMapJoints.Count());
		}

		public static void LoadMapCapsuleItemInfo()
		{
			ConcurrentDictionary<int, List<MapCapsuleItemInfo>> concurrentDictionary = new ConcurrentDictionary<int, List<MapCapsuleItemInfo>>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_loadCapsuleItemInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					MapCapsuleItemInfo item = new MapCapsuleItemInfo
					{
						PresentRuleType = Convert.ToInt32(mySqlDataReader["fdPresentRuleType"]),
						Argument = Convert.ToInt32(mySqlDataReader["fdArgument"]),
						GameItemNum = Convert.ToInt32(mySqlDataReader["fdGameItemNum"]),
						Rate = Convert.ToInt32(mySqlDataReader["fdRate"])
					};
					int key = Convert.ToInt32(mySqlDataReader["fdGroupNum"]);
					concurrentDictionary.AddOrUpdate(key, new List<MapCapsuleItemInfo> { item }, delegate(int k, List<MapCapsuleItemInfo> v)
					{
						v.Add(item);
						return v;
					});
				}
			}
			foreach (KeyValuePair<int, List<MapCapsuleItemInfo>> item2 in concurrentDictionary)
			{
				IWeightedRandomizer<MapCapsuleItemInfo> weightedRandomizer = new StaticWeightedRandomizer<MapCapsuleItemInfo>();
				foreach (MapCapsuleItemInfo item3 in item2.Value)
				{
					weightedRandomizer.Add(item3, item3.Rate);
				}
				MapCapsuleItems.TryAdd(item2.Key, weightedRandomizer);
			}
			Log.Information("Load MapCapsuleItems Count: {0}", MapCapsuleItems.Count());
		}

		public static void LoadAssaultModeRewardInfo()
		{
			AssaultModeRewardInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_AssaultModeGetRewardInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["mapNum"]);
					int key2 = Convert.ToInt32(mySqlDataReader["groupNum"]);
					int key3 = Convert.ToInt32(mySqlDataReader["rewardType"]);
					AssaultModeRewardInfo value = new AssaultModeRewardInfo
					{
						rewardID = Convert.ToInt32(mySqlDataReader["rewardID"]),
						rewardRate = Convert.ToInt32(mySqlDataReader["rewardRate"]),
						minValue = Convert.ToInt32(mySqlDataReader["minValue"]),
						maxValue = Convert.ToInt32(mySqlDataReader["maxValue"])
					};
					AssaultModeRewardInfos[key][key2][key3] = value;
				}
			}
			Log.Information("Load AssaultModeRewardInfo Count: {0}", AssaultModeRewardInfos.Count);
		}
	}
}
