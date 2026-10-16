using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.IceFlower;
using Serilog;

namespace RoomServer.Holders
{
	public static class IceFlowerHolder
	{
		public static List<IceFlowerText> IceFlowerTexts = new List<IceFlowerText>();

		public static void LoadIceFlowerInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from tbliceflowertext;", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					IceFlowerTexts.Add(new IceFlowerText
					{
						MapNum = Convert.ToInt32(mySqlDataReader["fdMapNum"]),
						Index = Convert.ToInt32(mySqlDataReader["fdIndex"]),
						Text = mySqlDataReader["fdText"].ToString()
					});
				}
			}
			Log.Information("Load IceFlower Count: {0}", IceFlowerTexts.Count());
		}
	}
}
