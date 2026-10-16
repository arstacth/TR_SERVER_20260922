using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Structuring.Park;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using Serilog;

namespace AgentServer.Holders
{
	public static class EventPickBoardHolder
	{
		public static HashSet<int> DiceBoardList = new HashSet<int>();

		public static ConcurrentDictionary<int, EventPickBoardInfo> EventPickBoardInfo { get; } = new ConcurrentDictionary<int, EventPickBoardInfo>();


		public static NestedDictionary<int, byte, float> EventPickBoardBasicInfo { get; } = new NestedDictionary<int, byte, float>();


		public static ConcurrentDictionary<int, HuMongPickBoardData> HuMongPickBoardContainer { get; set; } = new ConcurrentDictionary<int, HuMongPickBoardData>();


		public static void LoadEventPickBoardInfo()
		{
			try
			{
				EventPickBoardInfo.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_eventPickBoardInfo";
					mySqlCommand.Parameters.Add("PickBoardNum", MySqlDbType.Int32).Value = 0;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("PickBoardNum");
						DateTime dateTime = mySqlDataReader.GetDateTime("StartDateTime");
						DateTime dateTime2 = mySqlDataReader.GetDateTime("EndDateTime");
						int int2 = mySqlDataReader.GetInt32("ConstructType");
						int int3 = mySqlDataReader.GetInt32("StepUpturnType");
						int int4 = mySqlDataReader.GetInt32("StepResetType");
						EventPickBoardInfo value = new EventPickBoardInfo
						{
							StartDateTime = dateTime,
							EndDateTime = dateTime2,
							ConstructType = int2,
							StepUpturnType = int3,
							StepResetType = int4
						};
						EventPickBoardInfo.TryAdd(@int, value);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						int int5 = mySqlDataReader.GetInt32("PickBoardNum");
						byte @byte = mySqlDataReader.GetByte("PickBoardStep");
						float @float = mySqlDataReader.GetFloat("Rate");
						EventPickBoardBasicInfo[int5][@byte] = @float;
					}
				}
				foreach (KeyValuePair<int, EventPickBoardInfo> item in EventPickBoardInfo)
				{
					item.Value.LastStep = EventPickBoardBasicInfo[item.Key].Keys.OrderByDescending((byte o) => o).FirstOrDefault();
				}
				Log.Information("Load EventPickBoardInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load EventPickBoardInfo Error : {0}", ex.Message);
			}
		}

		public static void LoadHuMongPickBoardInfo()
		{
			try
			{
				HuMongPickBoardContainer.Clear();
				NestedDictionary<int, int, HuMongPickBoardItemInfo> nestedDictionary = new NestedDictionary<int, int, HuMongPickBoardItemInfo>();
				NestedDictionary<int, short, bool> nestedDictionary2 = new NestedDictionary<int, short, bool>();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_HuMongPickBoard_GetInfo";
					mySqlCommand.Parameters.Add("PickBoardNum", MySqlDbType.Int32).Value = 0;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("fdPickBoardNum");
						int int2 = mySqlDataReader.GetInt32("fdItemNum");
						HuMongPickBoardItemInfo value = new HuMongPickBoardItemInfo
						{
							ItemNum = int2,
							ItemCount = mySqlDataReader.GetInt16("fdItemCount"),
							ItemMax = mySqlDataReader.GetInt16("fdItemMax"),
							Rank = mySqlDataReader.GetByte("fdRank"),
							ResetCount = mySqlDataReader.GetInt32("fdResetCount"),
							LastResetTime = DateTime.Now
						};
						// MSSQL sync can yield duplicate (board,item); last row wins.
						nestedDictionary[@int][int2] = value;
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						int int3 = mySqlDataReader.GetInt32("fdPickBoardNum");
						short int4 = mySqlDataReader.GetInt16("fdPickID");
						bool boolean = mySqlDataReader.GetBoolean("fdIsPicked");
						nestedDictionary2[int3][int4] = boolean;
					}
				}
				foreach (KeyValuePair<int, NestedDictionary<int, HuMongPickBoardItemInfo>> item in nestedDictionary)
				{
					HuMongPickBoardData huMongPickBoardData = new HuMongPickBoardData();
					foreach (HuMongPickBoardItemInfo value2 in item.Value.Values)
					{
						huMongPickBoardData.AddToItemList(value2);
					}
					bool[] pickInfo = nestedDictionary2.ContainsKey(item.Key)
						? nestedDictionary2[item.Key].Values.ToArray()
						: new bool[0];
					huMongPickBoardData.UpdatePickBoardInfo(item.Key, pickInfo);
					HuMongPickBoardContainer.TryAdd(item.Key, huMongPickBoardData);
				}
				Log.Information("Load HuMongPickBoardInfo Done! boards={0}", HuMongPickBoardContainer.Count);
			}
			catch (Exception ex)
			{
				Log.Error("Load HuMongPickBoardInfo Error : {0}", ex.Message);
			}
		}

		public static void LoadDiceBoardOpenList()
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DiceBoard_GetBoardList";
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						DiceBoardList.Add(mySqlDataReader.GetInt32(0));
					}
				}
				Log.Information("Load DiceBoardOpenList Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load DiceBoardOpenList Error : {0}", ex.Message);
			}
		}
	}
}
