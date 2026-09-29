using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring;
using Serilog;

namespace RoomServer.Holders
{
	public static class GameModeHolder
	{
		public static ConcurrentDictionary<int, ConcurrentDictionary<int, List<CorunModeResult>>> CorunModeInfos { get; } = new ConcurrentDictionary<int, ConcurrentDictionary<int, List<CorunModeResult>>>();


		public static void LoadCorunModeResultInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getCorunModeResultPoint";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				ConcurrentDictionary<int, List<CorunModeResult>> concurrentDictionary = new ConcurrentDictionary<int, List<CorunModeResult>>();
				concurrentDictionary.Clear();
				int num = 0;
				while (mySqlDataReader.Read())
				{
					int num2 = num;
					num = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					int key = Convert.ToInt32(mySqlDataReader["fdResultType"]);
					if (num2 != num && num2 != 0)
					{
						ConcurrentDictionary<int, List<CorunModeResult>> value = new ConcurrentDictionary<int, List<CorunModeResult>>(concurrentDictionary);
						CorunModeInfos.TryAdd(num2, value);
						concurrentDictionary.Clear();
					}
					CorunModeResult resultinfo = new CorunModeResult
					{
						ResultType = Convert.ToInt32(mySqlDataReader["fdResultType"]),
						ResultPoint = Convert.ToByte(mySqlDataReader["fdResultPoint"]),
						TimeFrom = Convert.ToInt32(mySqlDataReader["fdTimeFrom"]) * 1000,
						TimeTo = Convert.ToInt32(mySqlDataReader["fdTimeTo"]) * 1000
					};
					concurrentDictionary.AddOrUpdate(key, new List<CorunModeResult> { resultinfo }, delegate(int k, List<CorunModeResult> v)
					{
						v.Add(resultinfo);
						return v;
					});
				}
				ConcurrentDictionary<int, List<CorunModeResult>> value2 = new ConcurrentDictionary<int, List<CorunModeResult>>(concurrentDictionary);
				CorunModeInfos.TryAdd(num, value2);
				concurrentDictionary.Clear();
			}
			foreach (KeyValuePair<int, ConcurrentDictionary<int, List<CorunModeResult>>> corunModeInfo in CorunModeInfos)
			{
				foreach (KeyValuePair<int, List<CorunModeResult>> item in corunModeInfo.Value)
				{
					foreach (CorunModeResult item2 in item.Value)
					{
						if (item.Key == 1)
						{
							if (item2.Equals(item.Value.Last()))
							{
								item2.TimeTo = 999999;
							}
						}
						else if (item.Key != 1 && item.Key != 3 && item2.Equals(item.Value.First()))
						{
							item2.TimeTo = 999999;
						}
					}
				}
			}
			Log.Information("Load CorunModeResultInfo Count: {0}", CorunModeInfos.Count());
		}

		public static CorunModeResult GetResultInfo(ConcurrentDictionary<int, List<CorunModeResult>> mapresultinfos, int resulttype, int time)
		{
			mapresultinfos.TryGetValue(resulttype, out var value);
			if (resulttype == 3)
			{
				return value.FirstOrDefault();
			}
			return value.Where((CorunModeResult w) => w.TimeFrom <= time && time < w.TimeTo).FirstOrDefault();
		}
	}
}
