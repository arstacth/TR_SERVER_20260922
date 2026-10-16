using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Structuring.HotTime;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class HotTimeHolder
	{
		public static Dictionary<long, HotTimeInfo> HotTimeInfos { get; set; } = new Dictionary<long, HotTimeInfo>();


		public static void LoadHotTimeInfo()
		{
			HotTimeInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_hotTimeNextInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					long key = Convert.ToInt64(mySqlDataReader["fdHotTimeID"]);
					HotTimeInfo value = new HotTimeInfo
					{
						HotTimeType = Convert.ToInt32(mySqlDataReader["fdHotTimeType"]),
						HotTimeImportant = Convert.ToInt32(mySqlDataReader["fdHotTimeImportant"]),
						StartTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdStartTime"])),
						FinishTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdFinishTime"])),
						RequiredTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdRequiredTime"])),
						LimitedUserNum = Convert.ToInt32(mySqlDataReader["fdLimitedUserNum"]),
						RewardKind = Convert.ToInt32(mySqlDataReader["fdRewardKind"])
					};
					HotTimeInfos.Add(key, value);
				}
			}
			using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection2.Open();
				using MySqlCommand mySqlCommand2 = new MySqlCommand(string.Empty, mySqlConnection2);
				mySqlCommand2.Parameters.Clear();
				mySqlCommand2.CommandType = CommandType.StoredProcedure;
				mySqlCommand2.CommandText = "usp_hotTimeRewardInfo";
				using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
				while (mySqlDataReader2.Read())
				{
					long key2 = Convert.ToInt64(mySqlDataReader2["fdHotTimeID"]);
					HotTimeRewardInfo item = new HotTimeRewardInfo
					{
						RewardType = Convert.ToInt32(mySqlDataReader2["fdRewardType"]),
						RewardValue = Convert.ToInt32(mySqlDataReader2["fdRewardValue"])
					};
					HotTimeInfos[key2].RewardInfos.Add(item);
				}
			}
			Log.Information("Load HotTimeInfo Done!");
		}
	}
}
