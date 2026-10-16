using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Map;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class SingleChallengeHolder
	{
		public static ConcurrentDictionary<int, List<ChallengeMapInfo>> MapInfos { get; } = new ConcurrentDictionary<int, List<ChallengeMapInfo>>();


		public static void LoadChallengeMapInfo()
		{
			MapInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essenchallengemodemapinfo;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					byte b = Convert.ToByte(mySqlDataReader["fdMedalType"]);
					if (b > 0)
					{
						ChallengeMapInfo info = new ChallengeMapInfo
						{
							MedalType = b,
							GoalSec = Convert.ToInt32(mySqlDataReader["fdGoalSec"])
						};
						MapInfos.AddOrUpdate(key, new List<ChallengeMapInfo> { info }, delegate(int k, List<ChallengeMapInfo> v)
						{
							v.Add(info);
							return v;
						});
					}
				}
			}
			Log.Information("Load SingleChallengeMapInfo Count: {0}", MapInfos.Count());
		}
	}
}
