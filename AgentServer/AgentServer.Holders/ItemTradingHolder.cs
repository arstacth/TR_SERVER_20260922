using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Structuring.Item;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class ItemTradingHolder
	{
		public static ConcurrentDictionary<int, List<ItemTradeInfo>> TradeInfo { get; } = new ConcurrentDictionary<int, List<ItemTradeInfo>>();


		public static void LoadItemTradeInfo()
		{
			try
			{
				Dictionary<int, List<ItemTradeInfo>> dictionary = new Dictionary<int, List<ItemTradeInfo>>();
				TradeInfo.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_ItemTrading_TradeInfo";
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("source_item");
						int ticket = mySqlDataReader.GetInt32("source_ticket");
						if (!dictionary.TryGetValue(@int, out var value))
						{
							value = new List<ItemTradeInfo>();
						}
						ItemTradeInfo itemTradeInfo;
						if (value.Any((ItemTradeInfo f) => f.m_ticket == ticket))
						{
							itemTradeInfo = value.FirstOrDefault((ItemTradeInfo f) => f.m_ticket == ticket);
						}
						else
						{
							itemTradeInfo = new ItemTradeInfo
							{
								m_result_count = mySqlDataReader.GetInt32("target_count"),
								m_min_unduplicated_item = mySqlDataReader.GetInt32("min_unduplicated"),
								m_ticket = ticket
							};
							value.Add(itemTradeInfo);
						}
						int int2 = mySqlDataReader.GetInt32("position");
						float @float = mySqlDataReader.GetFloat("position_ratio");
						itemTradeInfo.m_position2ratio[int2] = @float;
						int2 = mySqlDataReader.GetInt32("lvl");
						@float = mySqlDataReader.GetFloat("lvl_ratio");
						itemTradeInfo.m_levelratio[int2] = @float;
						dictionary[@int] = value;
					}
				}
				foreach (KeyValuePair<int, List<ItemTradeInfo>> item in dictionary)
				{
					TradeInfo[item.Key] = item.Value.ToList();
				}
				KeyValuePair<int, List<ItemTradeInfo>>[] array = dictionary.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<int, List<ItemTradeInfo>> keyValuePair = array[i];
					foreach (ItemTradeInfo p2 in keyValuePair.Value)
					{
						float num = 0f;
						foreach (KeyValuePair<int, float> item2 in p2.m_position2ratio)
						{
							num += item2.Value;
						}
						KeyValuePair<int, float>[] array2;
						if (num > 0f)
						{
							array2 = p2.m_position2ratio.ToArray();
							foreach (KeyValuePair<int, float> keyValuePair2 in array2)
							{
								TradeInfo[keyValuePair.Key].FirstOrDefault((ItemTradeInfo f) => f.m_ticket == p2.m_ticket).m_position2ratio[keyValuePair2.Key] /= num;
							}
						}
						num = 0f;
						foreach (KeyValuePair<int, float> item3 in p2.m_levelratio)
						{
							num += item3.Value;
						}
						if (!(num > 0f))
						{
							continue;
						}
						array2 = p2.m_levelratio.ToArray();
						foreach (KeyValuePair<int, float> keyValuePair3 in array2)
						{
							TradeInfo[keyValuePair.Key].FirstOrDefault((ItemTradeInfo f) => f.m_ticket == p2.m_ticket).m_levelratio[keyValuePair3.Key] /= num;
						}
					}
				}
				dictionary.Clear();
				dictionary = null;
				Log.Information("Load ItemTrading TradeInfo : {0}", TradeInfo.Count);
			}
			catch (Exception ex)
			{
				Log.Error("Load ItemTrading TradeInfo Error : {0}", ex.ToString());
			}
		}
	}
}
