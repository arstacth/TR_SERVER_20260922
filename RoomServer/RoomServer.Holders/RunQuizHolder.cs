using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Serilog;

namespace RoomServer.Holders
{
	public static class RunQuizHolder
	{
		public static Dictionary<int, string> RunQuizInfo = new Dictionary<int, string>();

		public static void LoadRunQuizInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essenrunquizmodedesc;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					RunQuizInfo.Add(Convert.ToInt32(mySqlDataReader["fdRunQuizNum"]), Convert.ToString(mySqlDataReader["fdName"]));
				}
			}
			Log.Information("Load RunQuizInfo Count: {0}", RunQuizInfo.Count());
		}
	}
}
