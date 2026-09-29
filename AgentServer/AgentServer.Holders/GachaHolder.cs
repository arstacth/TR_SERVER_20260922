using System;
using System.Collections.Generic;
using AgentServer.Structuring.Gacha;
using MySql.Data.MySqlClient;
using Serilog;
using Weighted_Randomizer;

namespace AgentServer.Holders
{
	public static class GachaHolder
	{
		public static bool HasSetGacha = false;

		public static int GachaId;

		public static string GachaName;

		public static DateTime StartDateTime;

		public static DateTime EndDateTime;

		public static IWeightedRandomizer<GachaItem> GachaItems = new StaticWeightedRandomizer<GachaItem>();

		public static List<GachaReward> GachaRewards = new List<GachaReward>();

		public static int RequiredItem;

		public static int FreeItem;

		public static void LoadGachaSetting()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from settingmonthlygacha where fdStartDateTime<=now() and fdEndDateTime>=now();", mySqlConnection);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					HasSetGacha = mySqlDataReader.HasRows;
					if (HasSetGacha)
					{
						GachaId = Convert.ToInt32(mySqlDataReader["fdGachaNum"]);
						GachaName = mySqlDataReader["fdGachaName"].ToString();
						StartDateTime = Convert.ToDateTime(mySqlDataReader["fdStartDateTime"]);
						EndDateTime = Convert.ToDateTime(mySqlDataReader["fdEndDateTime"]);
						RequiredItem = Convert.ToInt32(mySqlDataReader["fdRequiredItem"]);
						FreeItem = Convert.ToInt32(mySqlDataReader["fdFreeItem"]);
					}
				}
			}
			if (HasSetGacha)
			{
				LoadGachaItem();
				LoadGachaReward();
				Log.Information("Load Monthly Gacha Done!");
			}
			else
			{
				Log.Warning("Monthly Gacha Setting not found!");
			}
		}

		private static void LoadGachaItem()
		{
			GachaItems.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedatamonthlygachaitem where fdGachaNum=@gachaid;", mySqlConnection);
			mySqlCommand.Parameters.AddWithValue("@gachaid", GachaId);
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				GachaItem gachaItem = new GachaItem
				{
					ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
					Position = Convert.ToInt32(mySqlDataReader["fdPosition"]),
					Rate = Convert.ToInt32(mySqlDataReader["fdRate"])
				};
				GachaItems.Add(gachaItem, gachaItem.Rate);
			}
		}

		private static void LoadGachaReward()
		{
			GachaRewards.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedatamonthlygachareward where fdGachaNum=@gachaid;", mySqlConnection);
			mySqlCommand.Parameters.AddWithValue("@gachaid", GachaId);
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				GachaReward item = new GachaReward
				{
					ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
					RequiredTime = Convert.ToInt32(mySqlDataReader["fdRequiredTime"])
				};
				GachaRewards.Add(item);
			}
		}
	}
}
