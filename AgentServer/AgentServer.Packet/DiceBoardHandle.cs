using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Gacha;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class DiceBoardHandle
	{
		public static void Handle_GetDiceBoardList(ClientConnection Client, PacketReader reader, byte last)
		{
			Client.SendAsync(new GetDiceBoardList_ACK(last));
		}

		public static void Handle_GetDiceBoardUserInfoAndRewardInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int diceBoardNum = reader.ReadLEInt32();
			GetDiceBoardUserInfo(currentAccount.UserNum, diceBoardNum, out var DiceBoardType, out var CurrentPos, out var UserGauge, out var PauseRound);
			GetDiceBoardUserRewardInfo(currentAccount.UserNum, diceBoardNum, out var DiceBoardRewardList);
			Client.SendAsync(new GetDiceBoardUserInfoAndRewardInfo_ACK(diceBoardNum, DiceBoardType, UserGauge, CurrentPos, PauseRound, DiceBoardRewardList, last));
		}

		public static void Handle_GetDiceBoardUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int diceBoardNum = reader.ReadLEInt32();
			GetDiceBoardUserInfo(currentAccount.UserNum, diceBoardNum, out var _, out var _, out var UserGauge, out var _);
			Client.SendAsync(new GetDiceBoardUserInfo_ACK(diceBoardNum, UserGauge, last));
		}

		public static void Handle_DiceBoard_Draw(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int diceBoardNum = reader.ReadLEInt32();
			int num = 30;
			int minValue = 1;
			int maxValue = 4;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			if (random.Next(1, 101) <= num)
			{
				minValue = 4;
				maxValue = 7;
			}
			int DrawNum = random.Next(minValue, maxValue);
			int level = currentAccount.Level;
			if (DiceBoard_Draw(currentAccount, diceBoardNum, ref DrawNum, out var RewardPos, out var UserGauge, out var FinishedItem, out var isFinished, out var PauseRound, out var RewardItem))
			{
				Client.SendAsync(new DiceBoard_Draw_ACK(diceBoardNum, (byte)DrawNum, RewardPos, UserGauge, FinishedItem, isFinished, PauseRound, RewardItem, last));
				if (LobbyHandle.LevelUPCheck(currentAccount, level))
				{
					Client.SendAsync(new UserLevelUPEXPInfo(1, currentAccount.Level, currentAccount.Exp, last));
				}
			}
		}

		public static void Handle_DiceBoard_FillGauge(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int diceBoardNum = reader.ReadLEInt32();
			int itemNum = reader.ReadLEInt32();
			if (DiceBoard_FillGauge(currentAccount.UserNum, diceBoardNum, itemNum, out var UserGauge))
			{
				Client.SendAsync(new DiceBoard_FillGauge_ACK(diceBoardNum, UserGauge, itemNum, last));
			}
		}

		public static void Handle_DiceBoard_Reset(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int diceBoardNum = reader.ReadLEInt32();
			if (DiceBoard_Reset(currentAccount.UserNum, diceBoardNum, out var ResetItem) && GetDiceBoardUserInfo(currentAccount.UserNum, diceBoardNum, out var DiceBoardType, out var CurrentPos, out var UserGauge, out var PauseRound))
			{
				GetDiceBoardUserRewardInfo(currentAccount.UserNum, diceBoardNum, out var DiceBoardRewardList);
				Client.SendAsync(new DiceBoard_Reset_ACK(ResetItem, DiceBoardType, diceBoardNum, UserGauge, CurrentPos, PauseRound, DiceBoardRewardList, last));
			}
		}

		private static bool GetDiceBoardUserInfo(int UserNum, int DiceBoardNum, out int DiceBoardType, out int CurrentPos, out int UserGauge, out byte PauseRound)
		{
			DiceBoardType = 0;
			CurrentPos = 0;
			UserGauge = 0;
			PauseRound = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DiceBoard_GetUserInfo";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("DiceBoardNum", MySqlDbType.Int32).Value = DiceBoardNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						DiceBoardType = mySqlDataReader.GetInt32("DiceBoardType");
						CurrentPos = mySqlDataReader.GetInt32("CurrentPos");
						UserGauge = mySqlDataReader.GetInt32("UserGauge");
						PauseRound = Convert.ToByte(mySqlDataReader["PauseRound"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DiceBoard_GetUserInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static void GetDiceBoardUserRewardInfo(int UserNum, int DiceBoardNum, out List<DiceBoardReward> DiceBoardRewardList)
		{
			DiceBoardRewardList = new List<DiceBoardReward>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_DiceBoard_GetUserRewardInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("DiceBoardNum", MySqlDbType.Int32).Value = DiceBoardNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					DiceBoardReward item = new DiceBoardReward
					{
						Position = mySqlDataReader.GetInt32("fdPosition"),
						RewardItem = mySqlDataReader.GetInt32("fdRewardItem"),
						PositionType = mySqlDataReader.GetInt32("fdPositionType")
					};
					DiceBoardRewardList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_DiceBoard_GetUserRewardInfo Error:{0}", ex.Message);
			}
		}

		private static bool DiceBoard_Draw(Account User, int DiceBoardNum, ref int DrawNum, out int RewardPos, out int UserGauge, out int FinishedItem, out bool isFinished, out byte PauseRound, out List<int> RewardItem)
		{
			FinishedItem = 0;
			RewardPos = 0;
			UserGauge = 0;
			isFinished = false;
			RewardItem = new List<int>();
			PauseRound = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DiceBoard_Draw";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("DiceBoardNum", MySqlDbType.Int32).Value = DiceBoardNum;
					mySqlCommand.Parameters.Add("DrawNum", MySqlDbType.Int32).Value = DrawNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						FinishedItem = mySqlDataReader.GetInt32("FinishedItem");
						UserGauge = mySqlDataReader.GetInt32("UserGauge");
						isFinished = mySqlDataReader.GetBoolean("isFinished");
						PauseRound = Convert.ToByte(mySqlDataReader["PauseRound"]);
						DrawNum = mySqlDataReader.GetInt32("DrawNum");
						RewardPos = mySqlDataReader.GetInt32("RewardPos");
						RewardItem.Add(mySqlDataReader.GetInt32("RewardItem"));
						User.Exp = mySqlDataReader.GetInt64("totalExp");
						User.TR = mySqlDataReader.GetInt64("totalGameMoney");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DiceBoard_Draw Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool DiceBoard_FillGauge(int UserNum, int DiceBoardNum, int ItemNum, out int UserGauge)
		{
			UserGauge = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DiceBoard_FillGauge";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("DiceBoardNum", MySqlDbType.Int32).Value = DiceBoardNum;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						UserGauge = mySqlDataReader.GetInt32("UserGauge");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DiceBoard_FillGauge Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool DiceBoard_Reset(int UserNum, int DiceBoardNum, out int ResetItem)
		{
			ResetItem = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DiceBoard_ResetWithItem";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("DiceBoardNum", MySqlDbType.Int32).Value = DiceBoardNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						ResetItem = mySqlDataReader.GetInt32("ResetItem");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DiceBoard_ResetWithItem Error:{0}", ex.Message);
				return false;
			}
		}
	}
}
