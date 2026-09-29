using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.Item;
using Serilog;

namespace RoomServer.Holders
{
	public static class MapCardHolder
	{
		private static int[] doubleCardRate = new int[31]
		{
			0, 15, 15, 15, 15, 15, 15, 20, 30, 35,
			40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
			50, 55, 60, 65, 70, 75, 80, 85, 90, 95,
			100
		};

		private static double[,] cardRate = new double[31, 7]
		{
			{ 0.0, 30.0, 47.5, 15.0, 5.0, 2.0, 0.5 },
			{ 10.0, 26.0, 47.9, 17.0, 6.0, 2.5, 0.6 },
			{ 20.0, 22.0, 48.2, 19.0, 7.0, 3.0, 0.8 },
			{ 30.0, 19.0, 47.5, 21.0, 8.0, 3.5, 1.0 },
			{ 40.0, 16.0, 44.5, 23.0, 9.0, 6.0, 1.5 },
			{ 50.0, 14.0, 42.5, 25.0, 10.0, 6.5, 2.0 },
			{ 60.0, 12.0, 40.5, 27.0, 11.0, 7.0, 2.5 },
			{ 70.0, 11.0, 37.5, 29.0, 12.0, 7.5, 3.0 },
			{ 80.0, 10.0, 33.5, 30.0, 15.0, 8.0, 3.5 },
			{ 90.0, 9.5, 33.0, 29.0, 16.0, 8.5, 4.0 },
			{ 100.0, 9.0, 31.5, 28.0, 18.0, 9.0, 4.5 },
			{ 110.0, 8.0, 31.5, 27.5, 18.5, 9.5, 5.0 },
			{ 120.0, 7.0, 31.5, 27.0, 19.0, 10.0, 5.5 },
			{ 130.0, 6.0, 31.5, 26.5, 19.5, 10.5, 6.0 },
			{ 140.0, 5.0, 31.5, 26.0, 20.0, 11.0, 6.5 },
			{ 150.0, 4.0, 31.5, 25.5, 20.5, 11.5, 7.0 },
			{ 160.0, 3.0, 30.0, 25.0, 22.0, 12.0, 8.0 },
			{ 170.0, 2.9, 28.8, 24.8, 22.5, 12.5, 8.5 },
			{ 180.0, 2.8, 27.6, 24.6, 23.0, 13.0, 9.0 },
			{ 190.0, 2.7, 25.9, 24.4, 24.0, 13.5, 9.5 },
			{ 200.0, 2.5, 24.3, 24.2, 25.0, 14.0, 10.0 },
			{ 210.0, 2.4, 24.1, 24.0, 24.8, 14.5, 10.2 },
			{ 220.0, 2.3, 23.9, 23.8, 24.6, 15.0, 10.4 },
			{ 230.0, 2.2, 23.7, 23.6, 24.4, 15.5, 10.6 },
			{ 240.0, 2.1, 23.5, 23.4, 24.2, 16.0, 10.8 },
			{ 250.0, 1.9, 23.4, 23.2, 24.0, 16.5, 11.0 },
			{ 260.0, 1.8, 23.2, 23.0, 23.8, 17.0, 11.2 },
			{ 270.0, 1.7, 23.0, 22.8, 23.6, 17.5, 11.4 },
			{ 280.0, 1.6, 22.8, 22.6, 23.4, 18.0, 11.6 },
			{ 290.0, 1.5, 21.3, 22.4, 23.2, 19.8, 11.8 },
			{ 300.0, 1.0, 20.8, 22.2, 23.0, 20.0, 13.0 }
		};

		public static ConcurrentDictionary<int, List<MapCardRateInfo>> MapCardRateInfos2 { get; } = new ConcurrentDictionary<int, List<MapCardRateInfo>>();


		public static void LoadMapCardRateInfo()
		{
			new ConcurrentDictionary<int, List<MapCardRateInfo>>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_alchemist_getCardRateKindMapInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					MapCardRateInfo mapCardInfo = new MapCardRateInfo
					{
						CardNum = Convert.ToInt32(mySqlDataReader["carditemdesc"]),
						RateKind = Convert.ToInt32(mySqlDataReader["getratekind"])
					};
					MapCardRateInfos2.AddOrUpdate(Convert.ToInt32(mySqlDataReader["mapnum"]), new List<MapCardRateInfo> { mapCardInfo }, delegate(int k, List<MapCardRateInfo> v)
					{
						v.Add(mapCardInfo);
						return v;
					});
				}
			}
			Log.Information("Load MapCardRateInfo Count: {0}", MapCardRateInfos2.Count());
		}

		public static void GetCardProc(int UserNum, bool giveReward, int mapnum, float lucky, out List<int> takecard)
		{
			getCardListFromMapAndLucky(giveReward, mapnum, lucky, out takecard);
			foreach (int item in takecard)
			{
				if (item != 0)
				{
					giveCard(UserNum, item);
				}
			}
		}

		private static void getCardListFromMapAndLucky(bool giveReward, int mapnum, float lucky, out List<int> takecard)
		{
			takecard = new List<int>();
			if (giveReward)
			{
				int num = (int)((double)lucky / 10.0);
				if (num >= 0)
				{
					if (num > 30)
					{
						num = 30;
					}
				}
				else
				{
					num = 0;
				}
				int num2 = new Random(Guid.NewGuid().GetHashCode()).Next() % 100 + 1;
				int num3 = 1;
				if (num2 <= doubleCardRate[num])
				{
					num3 = 2;
				}
				for (int i = 0; i < num3; i++)
				{
					int cardItemDescNum = getCardItemDescNum(mapnum, lucky);
					takecard.Add(cardItemDescNum);
				}
			}
			else
			{
				takecard.Add(0);
			}
		}

		private static int getCardItemDescNum(int numMap, float fLucky)
		{
			double num = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * 100.0;
			int num2 = (int)(fLucky / 10f);
			if (num2 < 0)
			{
				num2 = 0;
			}
			else if (num2 > 30)
			{
				num2 = 30;
			}
			double num3 = 0.0;
			for (int i = 1; i < 7; i++)
			{
				num3 += cardRate[num2, i];
				if (num <= num3)
				{
					if (i == 1)
					{
						return 0;
					}
					return _getCardNumFromMapAndRateKind(numMap, i - 1);
				}
			}
			return 0;
		}

		private static int _getCardNumFromMapAndRateKind(int numMap, int iRateKind)
		{
			if (!MapCardRateInfos2.TryGetValue(numMap, out var value) || value.Count == 0)
			{
				return 0;
			}
			foreach (MapCardRateInfo item in value.OrderBy((MapCardRateInfo _) => Guid.NewGuid()))
			{
				if (item.RateKind == iRateKind)
				{
					return item.CardNum;
				}
			}
			return 0;
		}

		private static void giveCard(int usernum, int itemdescnum)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_alchemist_giveCard";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
			mySqlCommand.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemdescnum;
			mySqlCommand.ExecuteNonQuery();
		}
	}
}
