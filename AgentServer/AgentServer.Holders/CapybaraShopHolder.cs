using System;
using System.Collections.Generic;
using System.Data;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public struct CapybaraScheduleRow
	{
		public int ScheduleNum;
		public int ShopNum;
		public int DisplayStandNum;
		public long OpenTime;
		public long CloseTime;
	}

	public struct CapybaraUserTradeRow
	{
		public int ItemNum;
		public int Category;
		public int TradeCount;
	}

	public static class CapybaraShopHolder
	{
		public static void Load()
		{
			List<CapybaraScheduleRow> rows = LoadTodaySchedule(0);
			Log.Information("Load CapybaraShop schedule windows={0}", rows.Count);
		}

		public static List<CapybaraScheduleRow> LoadTodaySchedule(int shopNum)
		{
			List<CapybaraScheduleRow> list = new List<CapybaraScheduleRow>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(string.Empty, conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "usp_CapybaraShop_LoadData";
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					CapybaraScheduleRow row = default(CapybaraScheduleRow);
					row.ScheduleNum = Convert.ToInt32(reader["ScheduleNum"]);
					row.ShopNum = Convert.ToInt32(reader["fdShopNum"]);
					row.DisplayStandNum = Convert.ToInt32(reader["fdDisplayStandNum"]);
					row.OpenTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["fdOpen"]));
					row.CloseTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["fdClose"]));
					if (shopNum == 0 || row.ShopNum == shopNum)
					{
						list.Add(row);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_CapybaraShop_LoadData Error:{0}", ex.Message);
			}
			return list;
		}

		public static List<CapybaraUserTradeRow> GetUserInfo(int userNum, int shopNum, int scheduleNum)
		{
			List<CapybaraUserTradeRow> list = new List<CapybaraUserTradeRow>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(string.Empty, conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "usp_CapybaraShop_GetMayInfo";
				cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				cmd.Parameters.Add("shopNum", MySqlDbType.Int32).Value = shopNum;
				cmd.Parameters.Add("scheduleNum", MySqlDbType.Int32).Value = scheduleNum;
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					CapybaraUserTradeRow row = default(CapybaraUserTradeRow);
					row.ItemNum = Convert.ToInt32(reader["fdItemNum"]);
					row.Category = Convert.ToInt32(reader["fdCategory"]);
					row.TradeCount = Convert.ToInt32(reader["fdTradeCount"]);
					list.Add(row);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_CapybaraShop_GetMayInfo Error:{0}", ex.Message);
			}
			return list;
		}

		public static bool TradeItem(int userNum, int shopNum, int displayStand, int category, int scheduleNum, int itemNum, out int tradeCount)
		{
			tradeCount = -1;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(string.Empty, conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "usp_CapybaraShop_TradeItem";
				cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				cmd.Parameters.Add("shopNum", MySqlDbType.Int32).Value = shopNum;
				cmd.Parameters.Add("dpNum", MySqlDbType.Int16).Value = (short)displayStand;
				cmd.Parameters.Add("category", MySqlDbType.Byte).Value = (byte)category;
				cmd.Parameters.Add("scheduleNum", MySqlDbType.Int32).Value = scheduleNum;
				cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
				using MySqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					tradeCount = Convert.ToInt32(reader["fdTradeCount"]);
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_CapybaraShop_TradeItem Error:{0}", ex.Message);
			}
			return false;
		}
	}
}
