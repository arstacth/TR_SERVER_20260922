using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring;
using Serilog;

namespace RoomServer.Holders
{
	public static class ThankOfferingSystem
	{
		public static Dictionary<int, ThankOfferingSchedule> ThankOfferingSchedule = new Dictionary<int, ThankOfferingSchedule>();

		public static void LoadSchedule()
		{
			ThankOfferingEvent_GetSchedule();
			Log.Information("ThankOfferingSystem LoadSchedule:{0}", ThankOfferingSchedule.Count());
		}

		private static void ThankOfferingEvent_GetSchedule()
		{
			ThankOfferingSchedule.Clear();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_GetSchedule";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("scheduleNum");
					DateTime dateTime = mySqlDataReader.GetDateTime("startTime");
					int int2 = mySqlDataReader.GetInt32("duration");
					ThankOfferingSchedule value = new ThankOfferingSchedule
					{
						StartTime = dateTime,
						EndTime = dateTime.AddMinutes(int2)
					};
					ThankOfferingSchedule.Add(@int, value);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_GetSchedule Error: {0}", ex.Message);
			}
		}
	}
}
