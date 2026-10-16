using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Map;
using Serilog;
using Weighted_Randomizer;

namespace RoomServer.Holders
{
	public static class MapHolder
	{
		public static Dictionary<int, IWeightedRandomizer<BonusStageMapInfo>> BonusStageMapInfos = new Dictionary<int, IWeightedRandomizer<BonusStageMapInfo>>();

		public static NestedDictionary<int, int, int> BonusStageRankRewardInfos = new NestedDictionary<int, int, int>();

		public static Dictionary<int, List<GameRewardResult>> BonusStageRewardItemInfos = new Dictionary<int, List<GameRewardResult>>();

		public static ConcurrentDictionary<int, MapInfo> MapInfos { get; } = new ConcurrentDictionary<int, MapInfo>();


		public static ConcurrentDictionary<int, List<int>> MapRoomKinds { get; } = new ConcurrentDictionary<int, List<int>>();


		public static ConcurrentDictionary<int, int> AssaultModeLimitInfos { get; } = new ConcurrentDictionary<int, int>();


		public static ConcurrentDictionary<int, RunlympicMapInfo> RunlympicMapInfos { get; } = new ConcurrentDictionary<int, RunlympicMapInfo>();


		public static void LoadMapInfo()
		{
			MapInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getMapInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					MapInfo mapInfo = new MapInfo();
					mapInfo.CanTimeAttack = Convert.ToBoolean(mySqlDataReader["cantimeattack"]);
					mapInfo.RuleType = Convert.ToInt32(mySqlDataReader["ruletype"]);
					mapInfo.RewardLengthRate = Convert.ToSingle(mySqlDataReader["RewardLengthRate"]) / 100f;
					mapInfo.GoalInLimitTime = Convert.ToInt32(mySqlDataReader["goalInLimitLapTime"]);
					mapInfo.PlayMode = Convert.ToInt32(mySqlDataReader["playMode"]);
					mapInfo.Overhead = Convert.ToInt32(mySqlDataReader["overHead"]);
					mapInfo.LimitLapTime = Convert.ToInt32(mySqlDataReader["limitLapTime"]);
					mapInfo.RuleType_New = mySqlDataReader["ruletype_new"].ToString().Split('/');
					MapInfo value = mapInfo;
					int key = Convert.ToInt32(mySqlDataReader["mapnum"]);
					MapInfos.TryAdd(key, value);
				}
			}
			Log.Information("Load MapInfo Count: {0}", MapInfos.Count());
		}

		public static void LoadMapRoomKind()
		{
			MapRoomKinds.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essenmapnumbyroomkind;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdRoomKindID"]);
					int MapNum = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					MapRoomKinds.AddOrUpdate(key, new List<int> { MapNum }, delegate(int k, List<int> v)
					{
						v.Add(MapNum);
						return v;
					});
				}
			}
			Log.Information("Load MapRoomKind Count: {0}", MapRoomKinds.Count);
		}

		public static void LoadAssaultModeLimitInfo()
		{
			AssaultModeLimitInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from EssenAssaultModeLimitInfo;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdRoomKindID"]);
					int value = Convert.ToInt32(mySqlDataReader["fdAttackLimit"]);
					AssaultModeLimitInfos.TryAdd(key, value);
				}
			}
			Log.Information("Load AssaultModeAttackLimit Done!");
		}

		public static void LoadRunlympicMapInfo()
		{
			RunlympicMapInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essenrunlympicmap;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					int lapLimitTime = Convert.ToInt32(mySqlDataReader["fdLapLimitTime"]);
					int numLaps = Convert.ToInt32(mySqlDataReader["fdNumLaps"]);
					RunlympicMapInfo value = new RunlympicMapInfo
					{
						LapLimitTime = lapLimitTime,
						NumLaps = numLaps
					};
					RunlympicMapInfos.TryAdd(key, value);
				}
			}
			Log.Information("Load RunlympicMapInfo Done!");
		}

		public static void LoadBonusStageInfo()
		{
			BonusStageMapInfos.Clear();
			BonusStageRankRewardInfos.Clear();
			BonusStageRewardItemInfos.Clear();
			Dictionary<int, List<BonusStageMapInfo>> dictionary = new Dictionary<int, List<BonusStageMapInfo>>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getBonusStageInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("fdMapPlayerNum");
					BonusStageMapInfo item = new BonusStageMapInfo
					{
						MapNum = mySqlDataReader.GetInt32("fdMapNum"),
						LevelType = mySqlDataReader.GetInt32("fdLevelType"),
						Rate = mySqlDataReader.GetInt32("fdRate")
					};
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, new List<BonusStageMapInfo> { item });
					}
					else
					{
						dictionary[@int].Add(item);
					}
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					int int2 = mySqlDataReader.GetInt32("fdPlayerNum");
					int int3 = mySqlDataReader.GetInt32("fdPlayerRank");
					int int4 = mySqlDataReader.GetInt32("fdItemRank");
					BonusStageRankRewardInfos.Add(int2, int3, int4);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					int int5 = mySqlDataReader.GetInt32("fdItemRank");
					GameRewardResult item2 = new GameRewardResult
					{
						RewardType = Convert.ToInt16(mySqlDataReader["fdRewardType"]),
						RewardID = mySqlDataReader.GetInt32("fdRewardID"),
						RewardAmount = mySqlDataReader.GetInt32("fdAmount")
					};
					if (!BonusStageRewardItemInfos.ContainsKey(int5))
					{
						BonusStageRewardItemInfos.Add(int5, new List<GameRewardResult> { item2 });
					}
					else
					{
						BonusStageRewardItemInfos[int5].Add(item2);
					}
				}
			}
			foreach (KeyValuePair<int, List<BonusStageMapInfo>> item3 in dictionary)
			{
				IWeightedRandomizer<BonusStageMapInfo> weightedRandomizer = new StaticWeightedRandomizer<BonusStageMapInfo>();
				foreach (BonusStageMapInfo item4 in item3.Value)
				{
					weightedRandomizer.Add(item4, item4.Rate);
				}
				BonusStageMapInfos.Add(item3.Key, weightedRandomizer);
			}
			Log.Information("Load BonusStageInfo Done!");
		}

		public static int ResolveChangeMap(int roomKindId, int requested, int current)
		{
			if (requested > 0)
			{
				return requested;
			}
			if (!MapRoomKinds.TryGetValue(roomKindId, out var list) || list == null || list.Count == 0)
			{
				return current > 0 ? current : 0;
			}
			int idx = list.IndexOf(current);
			if (idx < 0)
			{
				return list[0];
			}
			return list[(idx + 1) % list.Count];
		}

	}
}
