using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Structuring.Farm;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class FarmRoomHandle
	{
		public static void GetFarmCraftMapDataInfo(int FarmUniqueNum, out FarmCraftMapData farmcraftmapdata)
		{
			farmcraftmapdata = new FarmCraftMapData();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_FarmCraft_GetFarmMapDataInfo";
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					farmcraftmapdata.isFarmCraft = true;
					farmcraftmapdata.TotalBlock = Convert.ToInt32(mySqlDataReader["FarmCraftBlockCount"]);
					byte[] array = new byte[Convert.ToInt32(mySqlDataReader["DataSize"])];
					mySqlDataReader.GetBytes(mySqlDataReader.GetOrdinal("FarmCraftCompressedData"), 0L, array, 0, array.Length);
					farmcraftmapdata.CompressedData = array;
				}
				else
				{
					farmcraftmapdata.isFarmCraft = false;
					farmcraftmapdata.TotalBlock = 0;
					farmcraftmapdata.CompressedData = new byte[0];
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_FarmCraft_GetFarmMapDataInfo error: {0}", ex.Message);
			}
		}

		public static void GetCurrentMapFarmCraftItemInfo(int FarmUniqueNum, int ItemNum, out List<FarmCraftMapItem> farmcraftmapitem)
		{
			farmcraftmapitem = new List<FarmCraftMapItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_FarmCraft_GetCurrentFarmItemInfo";
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
				mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					FarmCraftMapItem item = new FarmCraftMapItem
					{
						ItemKind = Convert.ToByte(mySqlDataReader["FarmCraftItemKind"]),
						TotalCount = Convert.ToInt32(mySqlDataReader["FarmCraftItemTotalCount"]),
						UsedCount = Convert.ToInt32(mySqlDataReader["FarmCraftItemUsedCount"])
					};
					farmcraftmapitem.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_FarmCraft_GetCurrentFarmItemInfo error: {0}", ex.Message);
			}
		}
	}
}
