using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.Room;
using Serilog;

namespace RoomServer.Holders
{
	public static class RoomHolder
	{
		public static ConcurrentDictionary<int, RoomKindInfo> RoomKindInfos { get; } = new ConcurrentDictionary<int, RoomKindInfo>();


		public static ConcurrentDictionary<int, RoomKind_UserMinMax> RoomKindPlayerNum { get; } = new ConcurrentDictionary<int, RoomKind_UserMinMax>();


		public static void LoadRoomKindInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getRoomKindID";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					RoomKindInfo value = new RoomKindInfo
					{
						GameMode = Convert.ToInt32(mySqlDataReader["GameMode"]),
						Channel = Convert.ToInt32(mySqlDataReader["Channel"]),
						PlayMode = Convert.ToInt32(mySqlDataReader["PlayMode"])
					};
					RoomKindInfos.TryAdd(Convert.ToInt32(mySqlDataReader["RoomKindID"]), value);
				}
			}
			Log.Information("Load RoomKindInfo Count: {0}", RoomKindInfos.Count());
			RoomKindPlayerNum.Clear();
			try
			{
				using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection2.Open();
					using MySqlCommand mySqlCommand2 = new MySqlCommand("select * from EssenCompetitionEvent_PlayerCount where fdUse=1;", mySqlConnection2);
					mySqlCommand2.Parameters.Clear();
					using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
					while (mySqlDataReader2.Read())
					{
						int @int = mySqlDataReader2.GetInt32("fdRoomKindID");
						int int2 = mySqlDataReader2.GetInt32("fdMin");
						int int3 = mySqlDataReader2.GetInt32("fdMax");
						RoomKind_UserMinMax value2 = new RoomKind_UserMinMax
						{
							MinUser = int2,
							MaxUser = int3
						};
						RoomKindPlayerNum.TryAdd(@int, value2);
					}
				}
				Log.Information("Load RoomKindPlayerNum Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load RoomKindPlayerNum Error:{0}", ex.ToString());
			}
		}
	}
}
