using System;
using System.Collections.Generic;
using System.Data;
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
	public class ExchangeHandle
	{
		public static void Handle_GetExchangeSystemInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (ItemHolder.ExchangeSystemInfo.TryGetValue(num, out var value) && value)
			{
				Client.SendAsync(new GetExchangeSystemOK(currentAccount, num, last));
			}
		}

		public static void Handle_ExchangeItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int key = reader.ReadLEInt32();
			int exchangeID = reader.ReadLEInt32();
			int count = reader.ReadLEInt32();
			if (ItemHolder.ExchangeSystemInfo.TryGetValue(key, out var value) && value && ExchangeItemEx(currentAccount, exchangeID, count, out var exinfo))
			{
				Client.SendAsync(new ExchangeItem(exinfo, count, last));
			}
			else
			{
				Client.SendAsync(new ExchangeItem(new List<ExchangeItemInfo>(), count, last));
			}
		}

		private static bool ExchangeItem(Account User, int ExchangeID, out List<ExchangeItemInfo> exinfo)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_exchangeSystem_Exchange";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("exchangeID", MySqlDbType.Int32).Value = ExchangeID;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							string text = mySqlDataReader["resultType"].ToString();
							if (text == "0")
							{
								if (Convert.ToInt32(mySqlDataReader["type"]) == 200)
								{
									User.TR -= Convert.ToInt32(mySqlDataReader["count"]);
								}
							}
							else if (text == "1")
							{
								ExchangeItemInfo item = new ExchangeItemInfo
								{
									type = Convert.ToInt32(mySqlDataReader["type"]),
									id = Convert.ToInt32(mySqlDataReader["id"]),
									count = Convert.ToInt32(mySqlDataReader["count"])
								};
								exinfo.Add(item);
							}
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("Error on exchange item:\r\n{0}", ex.Message);
				return false;
			}
		}

		private static bool ExchangeItemEx(Account User, int ExchangeID, int count, out List<ExchangeItemInfo> exinfo)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_exchangeSystem_ExchangeEx";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("exchangeID", MySqlDbType.Int32).Value = ExchangeID;
					mySqlCommand.Parameters.Add("pCount", MySqlDbType.Int32).Value = count;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							string text = mySqlDataReader["resultType"].ToString();
							if (text == "0")
							{
								if (Convert.ToInt32(mySqlDataReader["type"]) == 200)
								{
									User.TR -= Convert.ToInt32(mySqlDataReader["count"]);
								}
							}
							else if (text == "1")
							{
								ExchangeItemInfo item = new ExchangeItemInfo
								{
									type = Convert.ToInt32(mySqlDataReader["type"]),
									id = Convert.ToInt32(mySqlDataReader["id"]),
									count = Convert.ToInt32(mySqlDataReader["count"])
								};
								exinfo.Add(item);
							}
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("Error on exchange item:\r\n{0}", ex.Message);
				return false;
			}
		}
	}
}
