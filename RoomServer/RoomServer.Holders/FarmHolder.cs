using System;
using System.Collections.Concurrent;
using System.Linq;
using MySql.Data.MySqlClient;
using Serilog;

namespace RoomServer.Holders
{
	public static class FarmHolder
	{
		public static ConcurrentDictionary<int, int> FarmItemInfos { get; } = new ConcurrentDictionary<int, int>();


		public static void LoadFarmItemInfo()
		{
			FarmItemInfos.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM tblfarmitem", mySqlConnection);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					FarmItemInfos.TryAdd(Convert.ToInt32(mySqlDataReader["fdAvatarItemDescNum"]), Convert.ToInt32(mySqlDataReader["fdFarmItemPosition"]));
				}
			}
			Log.Information("Load FarmItemInfo Count: {0}", FarmItemInfos.Count());
		}
	}
}
