using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class ItemTradeHandle
	{
		public static void Handle_ItemTrading_Check(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (!ItemTradingHolder.TradeInfo.TryGetValue(num, out var value))
			{
				Client.SendAsync(new ITEM_TRADING_CHECK_ACK(value, new Dictionary<int, int>(), 0, eTRADE_CHECK_RESULT.eTRADE_CHECK_INVALID_ITEM, last));
				return;
			}
			ItemTrading_TicketCount(currentAccount.UserNum, out var ret, out var result);
			Client.SendAsync(new ITEM_TRADING_CHECK_ACK(value, ret, num, result, last));
		}

		public static void Handle_ItemTrading_Trade(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if ((currentAccount.TRADE_RESULT?.Count ?? 0) > 0)
			{
				Client.SendAsync(new ITEM_TRADING_TRADE_ACK(eTRADE_RESULT.eTRADE_RESULT_ON_PROCESSING, 0, 0, new List<TRADE_RESULT>(), last));
				return;
			}
			ItemTrading_Convert(currentAccount.UserNum, num2, num, out var trs, out var result);
			if (result == eTRADE_RESULT.eTRADE_RESULT_OK && trs.Count == 0)
			{
				result = eTRADE_RESULT.eTRADE_RESULT_FAILED_UNKNOWN;
			}
			if (result == eTRADE_RESULT.eTRADE_RESULT_OK)
			{
				currentAccount.TRADE_RESULT = trs;
				Client.SendAsync(new ITEM_TRADING_TRADE_ACK(result, num2, num, trs, last));
			}
			else
			{
				Client.SendAsync(new ITEM_TRADING_TRADE_ACK(result, 0, 0, new List<TRADE_RESULT>(), last));
			}
		}

		public static void Handle_ItemTrading_Complete(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int move_to_storage = reader.ReadByte();
			int target_item = reader.ReadLEInt32();
			List<TRADE_RESULT> tRADE_RESULT = currentAccount.TRADE_RESULT;
			if (tRADE_RESULT == null || tRADE_RESULT.Count == 0 || !currentAccount.TRADE_RESULT.Any((TRADE_RESULT a) => a.item == target_item))
			{
				Client.SendAsync(new ITEM_TRADING_COMPLETE_ACK(eTRADE_ACTION.eTRADE_ACTION_INVALID_MOVE_REQ, 0, 0, 0, last));
				return;
			}
			TRADE_RESULT tRADE_RESULT2 = currentAccount.TRADE_RESULT.FirstOrDefault((TRADE_RESULT f) => f.item == target_item);
			ItemTrading_Complete(currentAccount.UserNum, move_to_storage, tRADE_RESULT2, out var result);
			if (result == eTRADE_ACTION.eTRADE_ACTION_OK)
			{
				currentAccount.TRNeedUpdateFromDB = true;
				currentAccount.EXPNeedUpdateFromDB = true;
				Client.SendAsync(new ITEM_TRADING_COMPLETE_ACK(result, tRADE_RESULT2.src_item, tRADE_RESULT2.item, tRADE_RESULT2.src_ticket, last));
			}
			else
			{
				Client.SendAsync(new ITEM_TRADING_COMPLETE_ACK(result, 0, 0, 0, last));
			}
			currentAccount.TRADE_RESULT.Clear();
			currentAccount.TRADE_RESULT = null;
		}

		private static void ItemTrading_TicketCount(int UserNum, out Dictionary<int, int> ret, out eTRADE_CHECK_RESULT result)
		{
			result = eTRADE_CHECK_RESULT.eTRADE_CHECK_UNKNOWN;
			ret = new Dictionary<int, int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ItemTrading_TicketCount";
				mySqlCommand.Parameters.Add("user_num", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("fdItemDescNum");
					int int2 = mySqlDataReader.GetInt32("fdCount");
					ret.Add(@int, int2);
				}
				result = eTRADE_CHECK_RESULT.eTRADE_CHECK_OK;
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("Not Enough Capacity"))
				{
					result = eTRADE_CHECK_RESULT.eTRADE_CHECK_NOT_ENOUGH_STORAGE;
				}
				Log.Error("usp_ItemTrading_TicketCount Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_ItemTrading_TicketCount Error:{0}", ex2.ToString());
			}
		}

		private static void ItemTrading_Convert(int UserNum, int item, int ticket, out List<TRADE_RESULT> trs, out eTRADE_RESULT result)
		{
			result = eTRADE_RESULT.eTRADE_RESULT_FAILED_UNKNOWN;
			trs = new List<TRADE_RESULT>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ItemTrading_Convert2";
				mySqlCommand.Parameters.Add("user_num", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("itemnum", MySqlDbType.Int32).Value = item;
				mySqlCommand.Parameters.Add("ticket", MySqlDbType.Int32).Value = ticket;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						TRADE_RESULT item2 = new TRADE_RESULT
						{
							item = mySqlDataReader.GetInt32("item"),
							position = mySqlDataReader.GetInt32("POSITION"),
							level = mySqlDataReader.GetInt32("lvl"),
							can_move_to_storage = (mySqlDataReader.GetInt32("can_move_storage") != 0),
							src_item = item,
							src_ticket = ticket
						};
						trs.Add(item2);
					}
					result = eTRADE_RESULT.eTRADE_RESULT_OK;
				}
			}
			catch (MySqlException ex)
			{
				result = eTRADE_RESULT.eTRADE_RESULT_FAILED_WITH_ERROR_DESC;
				Log.Error("usp_ItemTrading_Convert2 Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_ItemTrading_Convert2 Error:{0}", ex2.ToString());
			}
		}

		public static void ItemTrading_Complete(int UserNum, int move_to_storage, TRADE_RESULT tr, out eTRADE_ACTION result)
		{
			result = eTRADE_ACTION.eTRADE_ACTION_OK;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ItemTrading_Complete";
				mySqlCommand.Parameters.Add("user_num", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("item", MySqlDbType.Int32).Value = tr.src_item;
				mySqlCommand.Parameters.Add("ticket", MySqlDbType.Int32).Value = tr.src_ticket;
				mySqlCommand.Parameters.Add("target_item", MySqlDbType.Int32).Value = tr.item;
				mySqlCommand.Parameters.Add("move_to_storage", MySqlDbType.Int32).Value = tr.can_move_to_storage && move_to_storage != 0;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (MySqlException ex)
			{
				result = eTRADE_ACTION.eTRADE_ACTION_ERROR_DESC;
				if (ex.Message.Contains("Not Enough Capacity"))
				{
					result = eTRADE_ACTION.eTRADE_ACTION_STORAGE_FULL;
				}
				Log.Error("usp_ItemTrading_Complete Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = eTRADE_ACTION.eTRADE_ACTION_UNKNOWN;
				Log.Error("usp_ItemTrading_Complete Error:{0}", ex2.ToString());
			}
		}
	}
}
