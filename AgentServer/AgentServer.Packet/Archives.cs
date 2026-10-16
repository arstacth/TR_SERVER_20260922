using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Gacha;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class Archives
	{
		public enum eArchives_Result
		{
			eArchives_Result_UNKNOWN,
			eArchives_Result_OK,
			eArchives_Result_NOT_ENOUGH_ITEM
		}

		public static void Handle_GetUserInfo(ClientConnection Client, byte last)
		{
			Archives_GetUserInfo(Client.CurrentAccount.UserNum, out var Infos);
			Client.SendAsync(new Archives_UserInfo_ACK(Infos, last));
		}

		public static void Handle_GetReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (Archives_GetReward(Index: reader.ReadLEInt32(), ItemNum: reader.ReadLEInt32(), UserNum: currentAccount.UserNum, exinfo: out var exinfo))
			{
				Client.SendAsync(new Archives_GiveReward_ACK(exinfo, last));
			}
		}

		public static void Handle_Archives_Exchange(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Archives_Exchange(ItemNum: reader.ReadLEInt32(), UserNum: currentAccount.UserNum, RewardItem: out var RewardItem, SupplyItemNum: out var SupplyItemNum, result: out var result);
			Client.SendAsync(new Archives_Exchange_ACK(result, RewardItem, SupplyItemNum, last));
		}

		private static void Archives_GetUserInfo(int UserNum, out List<ArchiveslInfo> Infos)
		{
			Infos = new List<ArchiveslInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Archives_GetUserInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ArchiveslInfo item = new ArchiveslInfo
					{
						Index = mySqlDataReader.GetInt32("IndexNum"),
						ItemNum = mySqlDataReader.GetInt32("ItemNum"),
						Count = mySqlDataReader.GetInt32("Count"),
						NeedCount = mySqlDataReader.GetInt32("RewardCount"),
						IsReceived = mySqlDataReader.GetBoolean("isReward")
					};
					Infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Archives_GetUserInfo Error:{0}", ex.Message);
			}
		}

		private static bool Archives_GetReward(int UserNum, int Index, int ItemNum, out List<ExchangeItemInfo> exinfo)
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
					mySqlCommand.CommandText = "usp_Archives_GetReward";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pIndex", MySqlDbType.Int32).Value = Index;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							ExchangeItemInfo item = new ExchangeItemInfo
							{
								type = mySqlDataReader.GetInt32("rewardType"),
								id = mySqlDataReader.GetInt32("rewardItem"),
								count = mySqlDataReader.GetInt32("rewardCount")
							};
							exinfo.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (MySqlException ex)
			{
				Log.Error("usp_Archives_GetReward Error:{0}", ex.Message);
				return false;
			}
			catch (Exception ex2)
			{
				Log.Error("usp_Archives_GetReward Error:{0}", ex2.Message);
				return false;
			}
		}

		private static void Archives_Exchange(int UserNum, int ItemNum, out List<ArchivesReward> RewardItem, out int SupplyItemNum, out eArchives_Result result)
		{
			result = eArchives_Result.eArchives_Result_OK;
			SupplyItemNum = 0;
			RewardItem = new List<ArchivesReward>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Archives_Exchange";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ArchivesReward item = new ArchivesReward
					{
						ItemNum = mySqlDataReader.GetInt32("RewardItemNum"),
						isGive = mySqlDataReader.GetBoolean("IsReal")
					};
					RewardItem.Add(item);
				}
				mySqlDataReader.NextResult();
				mySqlDataReader.Read();
				SupplyItemNum = mySqlDataReader.GetInt32("RefreshItemNum");
			}
			catch (MySqlException ex)
			{
				result = eArchives_Result.eArchives_Result_UNKNOWN;
				if (ex.Message.Contains("No have Items"))
				{
					result = eArchives_Result.eArchives_Result_NOT_ENOUGH_ITEM;
				}
				Log.Error("usp_Archives_Exchange Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = eArchives_Result.eArchives_Result_UNKNOWN;
				Log.Error("usp_Archives_Exchange Error:{0}", ex2.Message);
			}
		}
	}
}
