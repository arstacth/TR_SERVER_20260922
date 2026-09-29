using System;
using System.Collections.Concurrent;
using System.Data;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class QuizQuizHolder
	{
		public static int MAX_QUIZ = 10;

		public static ConcurrentDictionary<int, string> QuestionList { get; set; } = new ConcurrentDictionary<int, string>();


		public static void LoadQuizQuizInfo()
		{
			QuestionList.Clear();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_QuizQuiz_getQuestionList";
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("fdNum");
						string @string = mySqlDataReader.GetString("fdAnswer");
						QuestionList.TryAdd(@int, @string);
					}
				}
				Log.Information("Load QuizQuizInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load QuizQuizInfo Error:{0}", ex.Message);
			}
		}
	}
}
