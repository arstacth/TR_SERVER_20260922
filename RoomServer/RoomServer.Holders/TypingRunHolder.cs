using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.TypingRun;
using Serilog;

namespace RoomServer.Holders
{
	public static class TypingRunHolder
	{
		public static ConcurrentDictionary<int, int> TypingRunMaps = new ConcurrentDictionary<int, int>();

		public static List<TypingRunText> TypingRunTexts = new List<TypingRunText>();

		public static void LoadTypingRunData()
		{
			LoadTypingRunMap();
			LoadTypingRunText();
			Log.Information("Load Typing Run Data Done!");
		}

		private static void LoadTypingRunMap()
		{
			TypingRunMaps.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedatatypingrunmap;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
				int value = Convert.ToInt32(mySqlDataReader["fdAreaCount"]);
				TypingRunMaps.TryAdd(key, value);
			}
		}

		private static void LoadTypingRunText()
		{
			TypingRunTexts.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from tbltypingruntext;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				TypingRunTexts.Add(new TypingRunText
				{
					MapNum = Convert.ToInt32(mySqlDataReader["fdMapNum"]),
					Text = mySqlDataReader["fdText"].ToString()
				});
			}
		}
	}
}
