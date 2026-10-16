using System;
using System.Data;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.HotTime;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;

namespace AgentServer.Packet
{
	public class HotTimeHandle
	{
		public static void Handle_eServer_GET_HOTTIME_INFO_REQ(ClientConnection Client, byte last)
		{
			Client.SendAsync(new HotTimeInfos(last));
		}

		public static void Handle_ApplyHotTimeEvent(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt64();
			long num = reader.ReadLEInt64();
			bool flag = true;
			byte ret = 0;
			long num2 = Utility.CurrentTimeMilliseconds();
			if (!HotTimeHolder.HotTimeInfos.TryGetValue(num, out var value) || (value.StartTime > num2 && value.FinishTime < num2))
			{
				flag = false;
				ret = 1;
			}
			if (flag && CheckCanApply(currentAccount.UserNum, num, out ret))
			{
				ApplyHotTimeEvent(currentAccount.UserNum, num);
				Client.SendAsync(new ApplyHotTimeEventSuccess(num, last));
			}
			else
			{
				Client.SendAsync(new ApplyHotTimeEventFail(ret, num, last));
			}
		}

		public static void Handle_SetHotTimeInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			if (Client.CurrentAccount.Attribute != 0)
			{
				reader.Offset += 8;
				byte isUpdate = reader.ReadByte();
				int hotTimeType = reader.ReadLEInt32();
				int hotTimeImportant = reader.ReadLEInt32();
				long num = reader.ReadLEInt64();
				long finishTime = reader.ReadLEInt64();
				long requiredTime = reader.ReadLEInt64();
				int limitedUserNum = reader.ReadLEInt32();
				int rewardKind = reader.ReadLEInt32();
				HotTimeInfo hotTimeInfo = new HotTimeInfo
				{
					HotTimeType = hotTimeType,
					HotTimeImportant = hotTimeImportant,
					StartTime = num,
					FinishTime = finishTime,
					RequiredTime = requiredTime,
					LimitedUserNum = limitedUserNum,
					RewardKind = rewardKind
				};
				int num2 = reader.ReadLEInt32();
				HotTimeRewardInfoDB hotTimeRewardInfoDB = new HotTimeRewardInfoDB();
				if (num2 >= 1)
				{
					hotTimeRewardInfoDB.rewardType1 = reader.ReadLEInt32();
					hotTimeRewardInfoDB.rewardValue1 = reader.ReadLEInt32();
					HotTimeRewardInfo item = new HotTimeRewardInfo
					{
						RewardType = hotTimeRewardInfoDB.rewardType1,
						RewardValue = hotTimeRewardInfoDB.rewardValue1
					};
					hotTimeInfo.RewardInfos.Add(item);
				}
				if (num2 >= 2)
				{
					hotTimeRewardInfoDB.rewardType2 = reader.ReadLEInt32();
					hotTimeRewardInfoDB.rewardValue2 = reader.ReadLEInt32();
					HotTimeRewardInfo item2 = new HotTimeRewardInfo
					{
						RewardType = hotTimeRewardInfoDB.rewardType2,
						RewardValue = hotTimeRewardInfoDB.rewardValue2
					};
					hotTimeInfo.RewardInfos.Add(item2);
				}
				if (num2 >= 3)
				{
					hotTimeRewardInfoDB.rewardType3 = reader.ReadLEInt32();
					hotTimeRewardInfoDB.rewardValue3 = reader.ReadLEInt32();
					HotTimeRewardInfo item3 = new HotTimeRewardInfo
					{
						RewardType = hotTimeRewardInfoDB.rewardType3,
						RewardValue = hotTimeRewardInfoDB.rewardValue3
					};
					hotTimeInfo.RewardInfos.Add(item3);
				}
				if (num2 >= 4)
				{
					hotTimeRewardInfoDB.rewardType4 = reader.ReadLEInt32();
					hotTimeRewardInfoDB.rewardValue4 = reader.ReadLEInt32();
					HotTimeRewardInfo item4 = new HotTimeRewardInfo
					{
						RewardType = hotTimeRewardInfoDB.rewardType4,
						RewardValue = hotTimeRewardInfoDB.rewardValue4
					};
					hotTimeInfo.RewardInfos.Add(item4);
				}
				if (num2 >= 5)
				{
					hotTimeRewardInfoDB.rewardType5 = reader.ReadLEInt32();
					hotTimeRewardInfoDB.rewardValue5 = reader.ReadLEInt32();
					HotTimeRewardInfo item5 = new HotTimeRewardInfo
					{
						RewardType = hotTimeRewardInfoDB.rewardType5,
						RewardValue = hotTimeRewardInfoDB.rewardValue5
					};
					hotTimeInfo.RewardInfos.Add(item5);
				}
				long num3 = Convert.ToInt64(new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(num).ToLocalTime().ToString("yyyyMMddHHmm"));
				if (HotTimeHolder.HotTimeInfos.ContainsKey(num3))
				{
					isUpdate = 1;
				}
				HotTimeHolder.HotTimeInfos[num3] = hotTimeInfo;
				if (SetHotTimeEvent(isUpdate, num3, hotTimeInfo) && SetHotTimeEventReward(isUpdate, num3, hotTimeRewardInfoDB))
				{
					Client.SendAsync(new SetHotTimeEvent(num3, isUpdate, hotTimeInfo, last));
					ServerStatus.LBServerActor.Tell(new HTLoad());
				}
			}
		}

		public static void Handle_DeleteHotTimeEvent(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			long num = reader.ReadLEInt64();
			if (currentAccount.Attribute != 0 && DeleteHotTimeEvent(num))
			{
				Client.SendAsync(new HotTimeEventDelete(num, last));
				ServerStatus.LBServerActor.Tell(new HTLoad
				{
					type = 2,
					hottimeid = num
				});
			}
		}

		private static bool CheckCanApply(int Usernum, long HotTimeEventID, out byte ret)
		{
			ret = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_hotTimeCheckMyInfo";
				mySqlCommand.Parameters.Add("hottimeeventID", MySqlDbType.Int64).Value = HotTimeEventID;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = Usernum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				ret = (byte)mySqlDataReader.GetInt32("ret");
				if (ret == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool ApplyHotTimeEvent(int Usernum, long HotTimeEventID)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "\tusp_hotTimeEventApply";
				mySqlCommand.Parameters.Add("hottimeeventID", MySqlDbType.Int64).Value = HotTimeEventID;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = Usernum;
				mySqlCommand.ExecuteNonQuery();
			}
			return true;
		}

		private static bool SetHotTimeEvent(byte isUpdate, long HotTimeEventID, HotTimeInfo info)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_hotTimeSettingInfo";
				mySqlCommand.Parameters.Add("bUpdate", MySqlDbType.Int32).Value = isUpdate;
				mySqlCommand.Parameters.Add("hottimeid", MySqlDbType.Int64).Value = HotTimeEventID;
				mySqlCommand.Parameters.Add("eventType", MySqlDbType.Int16).Value = info.HotTimeType;
				mySqlCommand.Parameters.Add("eventImportant", MySqlDbType.Int16).Value = info.HotTimeImportant;
				mySqlCommand.Parameters.Add("startdatetime", MySqlDbType.VarString).Value = info.StartTime / 1000;
				mySqlCommand.Parameters.Add("finishdatetime", MySqlDbType.VarString).Value = info.FinishTime / 1000;
				mySqlCommand.Parameters.Add("requiredatetime", MySqlDbType.VarString).Value = info.RequiredTime / 1000;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = info.LimitedUserNum;
				mySqlCommand.Parameters.Add("giveType", MySqlDbType.Int16).Value = info.RewardKind;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					return true;
				}
			}
			return false;
		}

		private static bool SetHotTimeEventReward(byte isUpdate, long HotTimeEventID, HotTimeRewardInfoDB dbreward)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_hotTimeRewardSettingInfo";
				mySqlCommand.Parameters.Add("bUpdate", MySqlDbType.Int32).Value = isUpdate;
				mySqlCommand.Parameters.Add("hottimeid", MySqlDbType.Int64).Value = HotTimeEventID;
				mySqlCommand.Parameters.Add("rewardType1", MySqlDbType.Int32).Value = dbreward.rewardType1;
				mySqlCommand.Parameters.Add("rewardValue1", MySqlDbType.Int32).Value = dbreward.rewardValue1;
				mySqlCommand.Parameters.Add("rewardType2", MySqlDbType.Int32).Value = dbreward.rewardType2;
				mySqlCommand.Parameters.Add("rewardValue2", MySqlDbType.Int32).Value = dbreward.rewardValue2;
				mySqlCommand.Parameters.Add("rewardType3", MySqlDbType.Int32).Value = dbreward.rewardType3;
				mySqlCommand.Parameters.Add("rewardValue3", MySqlDbType.Int32).Value = dbreward.rewardValue3;
				mySqlCommand.Parameters.Add("rewardType4", MySqlDbType.Int32).Value = dbreward.rewardType4;
				mySqlCommand.Parameters.Add("rewardValue4", MySqlDbType.Int32).Value = dbreward.rewardValue4;
				mySqlCommand.Parameters.Add("rewardType5", MySqlDbType.Int32).Value = dbreward.rewardType5;
				mySqlCommand.Parameters.Add("rewardValue5", MySqlDbType.Int32).Value = dbreward.rewardValue5;
				mySqlCommand.ExecuteNonQuery();
			}
			return true;
		}

		private static bool DeleteHotTimeEvent(long HotTimeEventID)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_hotTimeDelete";
				mySqlCommand.Parameters.Add("hottimeid", MySqlDbType.Int64).Value = HotTimeEventID;
				mySqlCommand.ExecuteNonQuery();
			}
			return true;
		}
	}
}
