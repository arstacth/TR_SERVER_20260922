using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.GuildPlant;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public static class GuildPlantHandle
	{
		public static void Handle_GetGuildManageTR(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			long TR;
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetGuildManageTR(16, last));
			}
			else if (guildPlant_getManageTR(num, out TR))
			{
				Client.SendAsync(new GuildPlant_GetGuildManageTR(TR, last));
			}
		}

		public static void Handle_InvestGuildManageTR(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailInvestGuildManageTR(16, last));
				return;
			}
			if (num2 > 1000000)
			{
				num2 = 1000000;
			}
			long manageTR;
			short num3 = guildPlant_investTR(currentAccount, num2, out manageTR);
			switch (num3)
			{
			case 0:
				Client.SendAsync(new GuildPlant_InvestGuildManageTR(manageTR, currentAccount.TR, last));
				return;
			case -1:
				num3 = 0;
				break;
			}
			Client.SendAsync(new GuildPlant_FailInvestGuildManageTR(num3, last));
		}

		public static void Handle_GetStorageExtend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetStorageExtend(16, last));
				return;
			}
			guildPlant_getStorageExtend(currentAccount.UserNum, num, out var extendCount, out var _, out var userInvestCount, out var userInvestValue);
			Client.SendAsync(new GuildPlant_GetStorageExtend(extendCount, 10000000, userInvestCount, userInvestValue, last));
		}

		public static void Handle_RegisterItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int itemIndexNum = reader.ReadLEInt32();
			int distributeKind = reader.ReadLEInt32();
			int limitBuyCount = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailRegisterItem(16, last));
				return;
			}
			long manageTR;
			short num2 = guildPlant_registerItem(currentAccount.UserNum, itemIndexNum, distributeKind, limitBuyCount, out manageTR);
			if (num2 == 0)
			{
				Client.SendAsync(new GuildPlant_RegisterItem(manageTR, last));
			}
			else
			{
				Client.SendAsync(new GuildPlant_FailRegisterItem(num2, last));
			}
		}

		public static void Handle_GetMakeProgressItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (reader.ReadLEInt32() != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetMakeProgressItem(16, last));
				return;
			}
			int DistributeKind;
			int myContributionPoint;
			GuildPlantMakeInfo makeInfo = guildPlant_getMakeProgressItem(currentAccount.UserNum, out DistributeKind, out myContributionPoint);
			Client.SendAsync(new GuildPlant_GetMakeProgressItem(makeInfo, myContributionPoint, DistributeKind, last));
		}

		public static void Handle_GetMakeStandByItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetMakeStandByItemList(16, last));
				return;
			}
			guildPlant_getMakeStandByItemList(num, out var makeInfos);
			Client.SendAsync(new GuildPlant_GetMakeStandByItemList(makeInfos, last));
		}

		public static void Handle_ChangeMyConstributionPointItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int itemIndexNum = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailChangeMyConstributionPointItem(16, last));
				return;
			}
			short num2 = guildPlant_changeMyPointItem(currentAccount.UserNum, num, itemIndexNum);
			if (num2 == 0)
			{
				Client.SendAsync(new GuildPlant_ChangeMyConstributionPointItem(itemIndexNum, last));
			}
			else
			{
				Client.SendAsync(new GuildPlant_FailChangeMyConstributionPointItem(num2, last));
			}
		}

		public static void Handle_GetInvestorManageTRList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetInvestorManageTRList(16, last));
				return;
			}
			guildPlant_investorTRList(num, out var userList);
			Client.SendAsync(new GuildPlant_GetInvestorManageTRList(userList, last));
		}

		public static void Handle_GetExpenseList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int pointType = reader.ReadLEInt32();
			int month = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetExpenseList(16, last));
				return;
			}
			guildPlant_getExpenseList(num, pointType, month, out var useList);
			Client.SendAsync(new GuildPlant_GetExpenseList(pointType, month, useList, last));
		}

		public static void Handle_GetItemContributionRankList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int itemIndexNum = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetItemContributionRankList(16, last));
				return;
			}
			guildPlant_getItemPointRankList(num, itemIndexNum, out var userList);
			Client.SendAsync(new GuildPlant_GetItemContributionRankList(userList, last));
		}

		public static void Handle_GetGivePossibleUserList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int itemIndexNum = reader.ReadLEInt32();
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			bool o = reader.ReadBoolean();
			reader.ReadLEInt32();
			bool o2 = reader.ReadBoolean();
			reader.ReadLEInt32();
			bool o3 = reader.ReadBoolean();
			reader.ReadLEInt32();
			bool o4 = reader.ReadBoolean();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetGivePossibleUserList(16, last));
				return;
			}
			guildPlant_givePossibleUserList(num, itemIndexNum, o, o2, o3, o4, out var userList);
			Client.SendAsync(new GuildPlant_GetGivePossibleUserList(userList, last));
		}

		public static void Handle_GiveGift(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int itemIndexNum = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGiveGift(16, last));
				return;
			}
			if (num2 < 1)
			{
				Client.SendAsync(new GuildPlant_FailGiveGift(0, last));
				return;
			}
			string text = string.Empty;
			for (int i = 0; i < num2; i++)
			{
				short fixedLength = reader.ReadLEInt16();
				text += $"{reader.ReadBig5StringSafe(fixedLength)},";
			}
			short num3 = guildPlant_giveGift(currentAccount.UserNum, itemIndexNum, num2, text);
			if (num3 == 0)
			{
				Client.SendAsync(new GuildPlant_GiveGift(itemIndexNum, last));
			}
			else
			{
				Client.SendAsync(new GuildPlant_FailGiveGift(num3, last));
			}
		}

		public static void Handle_GetPlantItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailGetPlantItemList(16, last));
				return;
			}
			guildPlant_getSellInfoList(num, out var sellList);
			Client.SendAsync(new GuildPlant_GetPlantItemList(sellList, last));
		}

		public static void Handle_BuyItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int sellNum = reader.ReadLEInt32();
			if (num != currentAccount.GuildNum)
			{
				Client.SendAsync(new GuildPlant_FailBuyItem(16, last));
				return;
			}
			int ItemNum;
			int BuyCount;
			int myContributionPoint;
			short num2 = guildPlant_buyItem(currentAccount.UserNum, sellNum, out ItemNum, out BuyCount, out myContributionPoint);
			if (num2 == 0)
			{
				Client.SendAsync(new GuildPlant_BuyItem(sellNum, ItemNum, BuyCount, myContributionPoint, last));
			}
			else
			{
				Client.SendAsync(new GuildPlant_FailBuyItem(num2, last));
			}
		}

		private static bool guildPlant_getManageTR(int GuildNum, out long TR)
		{
			TR = 0L;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_getManageTR";
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = GuildNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						TR = mySqlDataReader.GetInt64("fdManageTR");
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getManageTR Error:{0}", ex.Message);
				return false;
			}
		}

		private static void guildPlant_getStorageExtend(int userNum, int GuildNum, out int extendCount, out int extendValue, out int userInvestCount, out int userInvestValue)
		{
			extendCount = 0;
			extendValue = 0;
			userInvestCount = 0;
			userInvestValue = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_getStorageExtend";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = GuildNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					extendCount = mySqlDataReader.GetInt32("extendCount");
					extendValue = mySqlDataReader.GetInt32("extendValue");
					userInvestCount = mySqlDataReader.GetInt32("userInvestCount");
					userInvestValue = mySqlDataReader.GetInt32("userInvestValue");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getStorageExtend Error:{0}", ex.Message);
			}
		}

		private static short guildPlant_investTR(Account User, int investTR, out long manageTR)
		{
			manageTR = 0L;
			short num = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_investTR";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("investTR", MySqlDbType.Int32).Value = investTR;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					num = (short)mySqlDataReader.GetInt32("ret");
					if (num == 0)
					{
						User.TR -= investTR;
						manageTR = Convert.ToInt64(mySqlDataReader["manageTR"]);
					}
				}
				return num;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_investTR Error:{0}", ex.Message);
				return -1;
			}
		}

		private static void guildPlant_investorTRList(int guildNum, out List<Tuple<int, string>> userList)
		{
			userList = new List<Tuple<int, string>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_investorTRList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					string @string = mySqlDataReader.GetString("nickname");
					int @int = mySqlDataReader.GetInt32("InvestTR");
					userList.Add(new Tuple<int, string>(@int, @string));
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_investorTRList Error:{0}", ex.Message);
			}
		}

		private static short guildPlant_registerItem(int UserNum, int itemIndexNum, int distributeKind, int limitBuyCount, out long manageTR)
		{
			manageTR = 0L;
			short num = 16;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_registerItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("itemIndexNum", MySqlDbType.Int32).Value = itemIndexNum;
					mySqlCommand.Parameters.Add("distributeKind", MySqlDbType.Int32).Value = distributeKind;
					mySqlCommand.Parameters.Add("limitBuyCount", MySqlDbType.Int32).Value = limitBuyCount;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows && mySqlDataReader.Read())
					{
						num = (short)mySqlDataReader.GetInt32("ret");
						if (num == 0)
						{
							manageTR = Convert.ToInt64(mySqlDataReader["manageTR"]);
						}
					}
				}
				return num;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_registerItem Error:{0}", ex.Message);
				return 7;
			}
		}

		private static void guildPlant_getMakeStandByItemList(int guildNum, out List<GuildPlantMakeInfo> makeInfos)
		{
			makeInfos = new List<GuildPlantMakeInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_getMakeStandByItemList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildPlantMakeInfo item = new GuildPlantMakeInfo
					{
						ItemIndexNum = mySqlDataReader.GetInt32("fdItemIdexNum"),
						ItemDescNum = mySqlDataReader.GetInt32("fdItemNum"),
						AccumulatePoint = mySqlDataReader.GetInt32("fdAccumulatePoint"),
						NeedPoint = mySqlDataReader.GetInt32("fdNeedPoint"),
						FinishDate = mySqlDataReader.GetDateTime("fdFinishDate")
					};
					makeInfos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getMakeStandByItemList Error:{0}", ex.Message);
			}
		}

		private static short guildPlant_changeMyPointItem(int UserNum, int guildNum, int itemIndexNum)
		{
			short result = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_changeMyPointItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
					mySqlCommand.Parameters.Add("itemIndexNum", MySqlDbType.Int32).Value = itemIndexNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					result = (short)mySqlDataReader.GetInt32("ret");
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_changeMyPointItem Error:{0}", ex.Message);
				return 11;
			}
		}

		private static GuildPlantMakeInfo guildPlant_getMakeProgressItem(int UserNum, out int DistributeKind, out int myContributionPoint)
		{
			GuildPlantMakeInfo result = null;
			DistributeKind = -1;
			myContributionPoint = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_getMakeProgressItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						result = new GuildPlantMakeInfo
						{
							ItemIndexNum = mySqlDataReader.GetInt32("fdItemIdexNum"),
							ItemDescNum = mySqlDataReader.GetInt32("fdItemNum"),
							AccumulatePoint = mySqlDataReader.GetInt32("fdAccumulatePoint"),
							NeedPoint = mySqlDataReader.GetInt32("fdNeedPoint"),
							FinishDate = mySqlDataReader.GetDateTime("fdFinishDate")
						};
						DistributeKind = mySqlDataReader.GetInt32("fdDistributeKind");
						myContributionPoint = mySqlDataReader.GetInt32("myContributionPoint");
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getMakeProgressItem Error:{0}", ex.Message);
				return result;
			}
		}

		private static void guildPlant_getItemPointRankList(int guildNum, int itemIndexNum, out List<Tuple<int, string>> userList)
		{
			userList = new List<Tuple<int, string>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_getItemPointRankList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				mySqlCommand.Parameters.Add("itemIndexNum", MySqlDbType.Int32).Value = itemIndexNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					string @string = mySqlDataReader.GetString("fdNickname");
					int @int = mySqlDataReader.GetInt32("fdPoint");
					userList.Add(new Tuple<int, string>(@int, @string));
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getItemPointRankList Error:{0}", ex.Message);
			}
		}

		private static void guildPlant_getSellInfoList(int guildNum, out List<GuildPlantSellInfo> sellList)
		{
			sellList = new List<GuildPlantSellInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_getSellInfoList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildPlantSellInfo item = new GuildPlantSellInfo
					{
						FinishDate = (mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("fdEndDate")) ? 0 : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("fdEndDate"))),
						SellNum = mySqlDataReader.GetInt32("fdSellNum"),
						ItemDescNum = mySqlDataReader.GetInt32("fdItemNum"),
						PointType = mySqlDataReader.GetInt32("fdSellPointKInd"),
						PointValue = mySqlDataReader.GetInt32("fdSellPointValue"),
						BuyCount = mySqlDataReader.GetInt32("fdBuyCount"),
						MaxCount = mySqlDataReader.GetInt32("fdMaxBuyCount")
					};
					sellList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getSellInfoList Error:{0}", ex.Message);
			}
		}

		private static short guildPlant_buyItem(int UserNum, int sellNum, out int ItemNum, out int BuyCount, out int myContributionPoint)
		{
			ItemNum = 0;
			BuyCount = 0;
			myContributionPoint = 0;
			short num = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_buyItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("sellNum", MySqlDbType.Int32).Value = sellNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					num = (short)mySqlDataReader.GetInt32("ret");
					if (num == 0)
					{
						ItemNum = Convert.ToInt32(mySqlDataReader["ItemNum"]);
						BuyCount = Convert.ToInt32(mySqlDataReader["BuyCount"]);
						myContributionPoint = Convert.ToInt32(mySqlDataReader["myContributionPoint"]);
					}
				}
				return num;
			}
			catch (Exception ex)
			{
				Log.Error("guildPlant_buyItem Error:{0}", ex.Message);
				return 12;
			}
		}

		private static void guildPlant_givePossibleUserList(int guildNum, int itemIndexNum, bool o1, bool o2, bool o3, bool o4, out List<string> userList)
		{
			userList = new List<string>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_givePossibleUserList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				mySqlCommand.Parameters.Add("itemIndexNum", MySqlDbType.Int32).Value = itemIndexNum;
				mySqlCommand.Parameters.Add("option1", MySqlDbType.Bit).Value = o1;
				mySqlCommand.Parameters.Add("option2", MySqlDbType.Bit).Value = o2;
				mySqlCommand.Parameters.Add("option3", MySqlDbType.Bit).Value = o3;
				mySqlCommand.Parameters.Add("option4", MySqlDbType.Bit).Value = o4;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					string @string = mySqlDataReader.GetString("fdNickName");
					userList.Add(@string);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_givePossibleUserList Error:{0}", ex.Message);
			}
		}

		private static void guildPlant_getExpenseList(int guildNum, int pointType, int month, out List<GuildPlantPointUseInfo> useList)
		{
			useList = new List<GuildPlantPointUseInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildPlant_getExpenseList";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				mySqlCommand.Parameters.Add("pointType", MySqlDbType.Int32).Value = pointType;
				mySqlCommand.Parameters.Add("month", MySqlDbType.Int32).Value = month;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildPlantPointUseInfo item = new GuildPlantPointUseInfo
					{
						DateTime = mySqlDataReader.GetDateTime("fdDateTime"),
						Memo = mySqlDataReader.GetString("fdDesc"),
						NickName = mySqlDataReader.GetString("fdNickName"),
						UsePoint = mySqlDataReader.GetInt32("fdUsePoint"),
						RemainPoint = mySqlDataReader.GetInt32("fdRemainPoint")
					};
					useList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_getExpenseList Error:{0}", ex.Message);
			}
		}

		private static short guildPlant_giveGift(int userNum, int itemIndexNum, int userCount, string userList)
		{
			short result = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guildPlant_giveGift";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("itemIndexNum", MySqlDbType.Int32).Value = itemIndexNum;
					mySqlCommand.Parameters.Add("userCount", MySqlDbType.Int32).Value = userCount;
					mySqlCommand.Parameters.Add("userList", MySqlDbType.VarString).Value = userList;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					result = (short)mySqlDataReader.GetInt32("ret");
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildPlant_giveGift Error:{0}", ex.Message);
				return 14;
			}
		}
	}
}
