using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.SiegeMode;
using Serilog;

namespace RoomServer.Holders
{
	public static class SiegeModeHolder
	{
		public static List<SiegeModeInfo> SiegeModeInfos { get; } = new List<SiegeModeInfo>();


		public static void LoadSiegeModeInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from EssenSiegeMode;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					SiegeModeInfos.Add(new SiegeModeInfo
					{
						MapNum = Convert.ToInt32(mySqlDataReader["fdMapNum"]),
						OptionType = Convert.ToInt32(mySqlDataReader["fdOptionType"]),
						Parameter = Convert.ToInt32(mySqlDataReader["fdParameter"]),
						Value = Convert.ToInt32(mySqlDataReader["fdValue"])
					});
				}
			}
			Log.Information("Load SiegeMode Info Count: {0}", SiegeModeInfos.Count());
		}
	}
}
