using System;
using System.Collections.Concurrent;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.SubjectKing;
using Serilog;

namespace RoomServer.Holders
{
	public static class SubjectKingHolder
	{
		public static ConcurrentDictionary<int, SubjectKingQuestionAnswer> SubjectKingQuestionAnswers = new ConcurrentDictionary<int, SubjectKingQuestionAnswer>();

		public static ConcurrentDictionary<int, int> SubjectKingMapSettings = new ConcurrentDictionary<int, int>();

		public static void LoadSubjectKingQuestionAnswer()
		{
			SubjectKingQuestionAnswers.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essensubjectkingquestionanswer where fdEnable=1;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdQuestionIndex"]);
					SubjectKingQuestionAnswer value = new SubjectKingQuestionAnswer
					{
						Question = mySqlDataReader["fdQuestion"].ToString(),
						Answer1 = mySqlDataReader["fdAnswer1"].ToString(),
						Answer2 = mySqlDataReader["fdAnswer2"].ToString(),
						Answer3 = mySqlDataReader["fdAnswer3"].ToString(),
						SubjectNum = Convert.ToInt32(mySqlDataReader["fdSubjectNum"])
					};
					SubjectKingQuestionAnswers.TryAdd(key, value);
				}
			}
			LoadSubjectKingMapSetting();
		}

		public static void LoadSubjectKingMapSetting()
		{
			SubjectKingMapSettings.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essensubjectkingmapsetting;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
					int value = Convert.ToInt32(mySqlDataReader["fdSubjectNum"]);
					SubjectKingMapSettings.TryAdd(key, value);
				}
			}
			Log.Information("Load Subject King Info Done!");
		}
	}
}
