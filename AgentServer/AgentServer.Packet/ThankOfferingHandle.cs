using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class ThankOfferingHandle
	{
		public static void Handle_Reward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int roomKind = reader.ReadLEInt32();
			if (ThankOfferingEvent_Reward(currentAccount.UserNum, roomKind, out var exinfo))
			{
				Client.SendAsync(new ThankOffering_Reward_ACK(roomKind, exinfo, last));
			}
		}

		public static void Handle_GetUserPoint(ClientConnection Client, PacketReader reader, byte last)
		{
			ThankOfferingEvent_GetUserPoint(Client.CurrentAccount.UserNum, out var normalPoint, out var hcPoint);
			Client.SendAsync(new ThankOffering_UserPoint_ACK(normalPoint, hcPoint, last));
		}

		public static void Handle_GetRank(ClientConnection Client, PacketReader reader, byte last)
		{
			ThankOfferingEvent_GetRank(Client.CurrentAccount.UserNum, out var ranklist);
			Client.SendAsync(new ThankOffering_Rank_ACK(ranklist, last));
		}

		private static void ThankOfferingEvent_GetUserPoint(int UserNum, out int normalPoint, out int hcPoint)
		{
			normalPoint = 10000;
			hcPoint = 10000;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_GetUserPoint";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				normalPoint = mySqlDataReader.GetInt32("normalPoint");
				hcPoint = mySqlDataReader.GetInt32("hcPoint");
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_GetUserPoint Error:{0}", ex.Message);
			}
		}

		private static bool ThankOfferingEvent_Reward(int UserNum, int RoomKind, out List<ExchangeItemInfo> exinfo)
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
					mySqlCommand.CommandText = "usp_ThankOfferingEvent_Reward";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("RoomKind", MySqlDbType.Int32).Value = RoomKind;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							ExchangeItemInfo item = new ExchangeItemInfo
							{
								type = mySqlDataReader.GetInt32("fdRewardType"),
								id = mySqlDataReader.GetInt32("fdRewardItem"),
								count = mySqlDataReader.GetInt32("fdRewardCount")
							};
							exinfo.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_Reward Error:{0}", ex.Message);
				return false;
			}
		}

		private static void ThankOfferingEvent_GetRank(int UserNum, out List<ThankOfferingRank> ranklist)
		{
			ranklist = new List<ThankOfferingRank>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_GetRank";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ThankOfferingRank item = new ThankOfferingRank
					{
						RoomKind = mySqlDataReader.GetInt32("fdRoomKind"),
						Rank = mySqlDataReader.GetInt32("fdRank"),
						NickName = mySqlDataReader.GetString("fdNickName"),
						EXP = mySqlDataReader.GetInt64("fdExp"),
						Point = mySqlDataReader.GetInt32("fdPoint"),
						isReward = mySqlDataReader.GetBoolean("fdIsReward")
					};
					ranklist.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_GetRank Error:{0}", ex.Message);
			}
		}
	}
}
