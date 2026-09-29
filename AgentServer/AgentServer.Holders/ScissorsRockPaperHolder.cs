using System;
using System.Collections.Concurrent;
using Akka.Actor;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class ScissorsRockPaperHolder
	{
		public static int PRIVATE_MAX_ROUND = 3;

		public static int GLOBAL_MAX_ROUND = 10;

		private static IActorRef GlobalRSPActor;

		public static ConcurrentDictionary<int, int> PrivateRSPReward { get; set; } = new ConcurrentDictionary<int, int>();


		public static ConcurrentDictionary<int, int> GlobalRSPReward { get; set; } = new ConcurrentDictionary<int, int>();


		public static void LoadScissorsRockPaperInfo()
		{
			PrivateRSPReward.Clear();
			GlobalRSPReward.Clear();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM EssenScissorsRockPaper_PrivateReward;", mySqlConnection);
					mySqlCommand.Parameters.Clear();
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("fdRound");
						int int2 = mySqlDataReader.GetInt32("fdItemNum");
						PrivateRSPReward.TryAdd(@int, int2);
					}
				}
				using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection2.Open();
					using MySqlCommand mySqlCommand2 = new MySqlCommand("SELECT * FROM EssenScissorsRockPaper_GlobalReward;", mySqlConnection2);
					mySqlCommand2.Parameters.Clear();
					using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
					while (mySqlDataReader2.Read())
					{
						int int3 = mySqlDataReader2.GetInt32("fdRound");
						int int4 = mySqlDataReader2.GetInt32("fdItemNum");
						GlobalRSPReward.TryAdd(int3, int4);
					}
				}
				Log.Information("Load RSPReward Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load PrivaeRSPReward Error:{0}", ex.Message);
			}
		}
	}
}
