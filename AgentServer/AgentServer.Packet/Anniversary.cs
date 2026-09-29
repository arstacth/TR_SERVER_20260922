using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Park;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class Anniversary
	{
		public static void Handle_ObjectAction(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int actionNum = reader.ReadLEInt32();
			int count = reader.ReadLEInt32();
			if (AnniversaryHolder.EventInfo.TryGetValue(num, out var value) && value.Exists((AnniversaryEvent e) => e.ActionNum == actionNum))
			{
				anniversaryAction(currentAccount, num, num, actionNum, count, out var addResult, out var _, out var exinfo);
				if (addResult > 0)
				{
					Client.SendAsync(new Anniversary_AddObjectValue_ACK(num, actionNum, count, addResult, last));
					Client.SendAsync(new Anniversary_InegratedRewardGiveReward_ACK(exinfo, num, actionNum, count, last));
				}
				if (addResult > 0 && AnniversaryHolder.ActionInfo.TryGetValue(actionNum, out var value2))
				{
					Client.SendAsync(new Anniversary_ObjectAction_ACK(num, actionNum, count, value2, last));
				}
			}
		}

		public static void Handle_GetObjectValue(ClientConnection Client, PacketReader reader, byte last)
		{
			anniversaryGetValue(reader.ReadLEInt32(), out var objectValueMap);
			if (objectValueMap.Count != 0)
			{
				Client.SendAsync(new Anniversary_GetObjectValue_ACK(objectValueMap, last));
			}
		}

		public static void Handle_GetReveivedRewardGradeList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			anniversaryGetValue(num, out var objectValueMap);
			anniversaryGetReceivedRewardGradeList(currentAccount.UserNum, num, out var grades);
			if (objectValueMap.Count != 0)
			{
				Client.SendAsync(new Anniversary_GetReveivedRewardGradeList_ACK(num, objectValueMap[num], grades, last));
			}
		}

		public static void Handle_ReceiveReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte grade = reader.ReadByte();
			if (anniversaryReceiveReward(currentAccount.UserNum, num, grade, out var rewardItemNum))
			{
				Client.SendAsync(new Anniversary_ReceiveReward_ACK(num, grade, rewardItemNum, last));
			}
		}

		private static void anniversaryAction(Account User, int anniversaryNum, int objectNum, int actionNum, int count, out long addResult, out long subResult, out List<ExchangeItemInfo> exinfo)
		{
			addResult = -1L;
			subResult = -1L;
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_anniversaryAction";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("lucky", MySqlDbType.Float).Value = User.Luck;
				mySqlCommand.Parameters.Add("anniversaryNum", MySqlDbType.Int32).Value = anniversaryNum;
				mySqlCommand.Parameters.Add("objectNum", MySqlDbType.Int32).Value = objectNum;
				mySqlCommand.Parameters.Add("actionNum", MySqlDbType.Int32).Value = actionNum;
				mySqlCommand.Parameters.Add("icount", MySqlDbType.Int32).Value = count;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					addResult = mySqlDataReader.GetInt64("resultValue");
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					subResult = mySqlDataReader.GetInt64("resultValue");
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					ExchangeItemInfo exchangeItemInfo = new ExchangeItemInfo
					{
						type = mySqlDataReader.GetInt32("rewardType"),
						id = mySqlDataReader.GetInt32("rewardItem"),
						count = mySqlDataReader.GetInt32("rewardCount")
					};
					if (exchangeItemInfo.type == 200)
					{
						User.TR += exchangeItemInfo.count;
					}
					else if (exchangeItemInfo.type == 500)
					{
						User.Exp += exchangeItemInfo.count;
					}
					exinfo.Add(exchangeItemInfo);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_anniversaryAction Error:{0}", ex.Message);
			}
		}

		private static void anniversaryGetValue(int anniversaryNum, out Dictionary<int, long> objectValueMap)
		{
			objectValueMap = new Dictionary<int, long>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_anniversaryGetValue";
				mySqlCommand.Parameters.Add("anniversaryNum", MySqlDbType.Int32).Value = anniversaryNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("anniversaryNum");
					long int2 = mySqlDataReader.GetInt64("value");
					objectValueMap.Add(@int, int2);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_anniversaryGetValue Error:{0}", ex.Message);
			}
		}

		private static void anniversaryGetReceivedRewardGradeList(int UserNum, int anniversaryNum, out List<byte> grades)
		{
			grades = new List<byte>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_anniversaryGetReceivedRewardGradeList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("anniversaryNum", MySqlDbType.Int32).Value = anniversaryNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					grades.Add(mySqlDataReader.GetByte("grade"));
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_anniversaryGetReceivedRewardGradeList Error:{0}", ex.Message);
			}
		}

		private static bool anniversaryReceiveReward(int UserNum, int anniversaryNum, byte grade, out int rewardItemNum)
		{
			rewardItemNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_anniversaryReceiveReward";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("anniversaryNum", MySqlDbType.Int32).Value = anniversaryNum;
					mySqlCommand.Parameters.Add("grade", MySqlDbType.Byte).Value = grade;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						rewardItemNum = mySqlDataReader.GetInt32("rewardItemNum");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_anniversaryReceiveReward Error:{0}", ex.Message);
				return false;
			}
		}
	}
}
