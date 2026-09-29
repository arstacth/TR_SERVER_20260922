using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using Akka.Actor;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using NetMsg.LBS;
using Serilog;

namespace AgentServer.Packet
{
	public class CombinationShopHandle
	{
		public static void Handle_GetUseInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			int num = reader.ReadLEInt32();
			if (num == 0)
			{
				Log.Error("Not found Exchange System Manager - SystemNum : {0}", num);
				return;
			}
			eCombinationShopResult useInfo = CombinationShopHolder.getUseInfo(num);
			Client.SendAsync(new COMBINATION_SHOP_GET_USE_INFO_ACK(num, useInfo, last));
		}

		public static void Handle_GetLimitCountInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int exchangeID = reader.ReadLEInt32();
			if (num == 0)
			{
				Log.Error("Not found Exchange System Manager - SystemNum : {0}", num);
			}
			else if (CombinationShopHolder.getUseInfo(num) == eCombinationShopResult.eCombinationShopResult_OK)
			{
				CombinationShop_UserCountInfo(currentAccount.UserNum, exchangeID, num, out var ExchangeCountList);
				Client.SendAsync(new COMBINATION_SHOP_GET_LIMIT_COUNT_INFO_ACK(num, exchangeID, ExchangeCountList, last));
			}
			else
			{
				Client.SendAsync(new COMBINATION_SHOP_GET_LIMIT_COUNT_INFO_ACK(num, exchangeID, null, last));
			}
		}

		public static void Handle_ShopExchange(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int exchangeID = reader.ReadLEInt32();
			if (num == 0)
			{
				Log.Error("Not found Exchange System Manager - SystemNum : {0}", num);
				return;
			}
			eCombinationShopResult useInfo = CombinationShopHolder.getUseInfo(num);
			if (useInfo == eCombinationShopResult.eCombinationShopResult_OK)
			{
				if (CombinationShopHolder.isExchangeCountCheck(num, exchangeID))
				{
					int eventNum = CombinationShopHolder.getEventNum(num);
					CombinationShop_Exchange(currentAccount, num, exchangeID, eventNum, last);
				}
				else
				{
					Client.SendAsync(new COMBINATION_SHOP_EXCHANGE_ACK(eCombinationShopResult.eCombinationShopResult_OVER_LIMIT_COUNT, bIsExhcangeSucess: false, null, null, last));
				}
			}
			else
			{
				Client.SendAsync(new COMBINATION_SHOP_EXCHANGE_ACK(useInfo, bIsExhcangeSucess: false, null, null, last));
			}
		}

		public static void Handle_ItemDetailInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Client.SendAsync(new COMBINATION_SHOP_GET_ITEM_DETAIL_INFO_ACK(last));
		}

		private static void CombinationShop_UserCountInfo(int UserNum, int exchangeID, int systemNum, out NestedDictionary<int, int, Tuple<int, int>> ExchangeCountList)
		{
			ExchangeCountList = new NestedDictionary<int, int, Tuple<int, int>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_CombinationShop_UserCountInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("exchangeID", MySqlDbType.Int32).Value = exchangeID;
				mySqlCommand.Parameters.Add("systemNum", MySqlDbType.Int32).Value = systemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("systemNum");
					int int2 = mySqlDataReader.GetInt32("exchangeID");
					int int3 = mySqlDataReader.GetInt32("maxSellCount");
					int int4 = mySqlDataReader.GetInt32("nowSellCount");
					if (ExchangeCountList.ContainsKey(@int))
					{
						ExchangeCountList[@int][int2] = new Tuple<int, int>(int3, int4);
					}
					else
					{
						ExchangeCountList.Add(@int, int2, new Tuple<int, int>(int3, int4));
					}
				}
			}
			catch (MySqlException ex)
			{
				Log.Error("usp_CombinationShop_UserCountInfo Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_CombinationShop_UserCountInfo Error:{0}", ex2.ToString());
			}
		}

		private static void CombinationShop_Exchange(Account User, int systemType, int exchangeID, int eventNum, byte last)
		{
			List<ExchangeItemInfo> list = new List<ExchangeItemInfo>();
			List<ExchangeItemInfo> list2 = new List<ExchangeItemInfo>();
			int num = 0;
			int limitCount = 0;
			bool flag = false;
			eCombinationShopResult eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_DBERROR;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_CombinationShop_Exchange";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("exchangeID", MySqlDbType.Int32).Value = exchangeID;
				mySqlCommand.Parameters.Add("eventNum", MySqlDbType.Int32).Value = eventNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("resultType");
					ExchangeItemInfo item = new ExchangeItemInfo
					{
						type = mySqlDataReader.GetInt32("type"),
						id = mySqlDataReader.GetInt32("id"),
						count = mySqlDataReader.GetInt32("count")
					};
					switch (@int)
					{
					case 0:
						list.Add(item);
						break;
					case 1:
						list2.Add(item);
						break;
					}
				}
				mySqlDataReader.NextResult();
				if (mySqlDataReader.Read())
				{
					num = mySqlDataReader.GetInt32("exchangeCount");
					limitCount = mySqlDataReader.GetInt32("limitCount");
					flag = mySqlDataReader.GetBoolean("isExchangeSucess");
				}
				eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_OK;
			}
			catch (MySqlException ex)
			{
				eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_DBERROR;
				if (ex.Message.Contains("not enough consume item") || ex.Message.Contains("Current point("))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_NOT_ENOUGH_CONSUME_ITEM;
				}
				else if (ex.Message.Contains("wear item"))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_WEAR_ITEM;
				}
				else if (ex.Message.Contains("already have item") || ex.Message.Contains("duplication") || ex.Message.Contains("already have pet"))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_AREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("over limit count"))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_OVER_LIMIT_COUNT;
				}
				else if (ex.Message.Contains("over limit user count"))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_OVER_LIMIT_USER_COUNT;
				}
				else if (ex.Message.Contains("not use time limit"))
				{
					eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_NOT_USE_TIME_LIMIT;
				}
				Log.Error("usp_CombinationShop_Exchange Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				eCombinationShopResult2 = eCombinationShopResult.eCombinationShopResult_DBERROR;
				Log.Error("usp_CombinationShop_Exchange Error:{0}", ex2.ToString());
			}
			if (eCombinationShopResult2 == eCombinationShopResult.eCombinationShopResult_OK && flag && num > 0)
			{
				ServerStatus.LBServerActor.Tell(new CombinationShopExchange
				{
					SystemType = systemType,
					ExchangeID = exchangeID,
					LimitCount = limitCount,
					ExchangeCount = num
				});
			}
			User.Connection.SendAsync(new COMBINATION_SHOP_EXCHANGE_ACK(eCombinationShopResult2, flag, list, list2, last));
		}
	}
}
