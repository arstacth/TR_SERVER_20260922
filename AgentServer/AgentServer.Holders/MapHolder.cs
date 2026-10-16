using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Structuring.Map;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class MapHolder
	{
		public static ConcurrentDictionary<int, MapInfo> MapInfos { get; } = new ConcurrentDictionary<int, MapInfo>();


		public static ConcurrentDictionary<int, List<int>> MapRoomKinds { get; } = new ConcurrentDictionary<int, List<int>>();


		public static ConcurrentDictionary<int, int> AssaultModeLimitInfos { get; } = new ConcurrentDictionary<int, int>();


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
	}
}
