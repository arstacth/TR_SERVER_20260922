using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Attendance;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class AttendanceHandle
	{
		public static void Handle_AttendanceInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			attendanceInfo(Client.CurrentAccount.UserNum, out var attendanceInfos, out var attendanceRewards);
			Client.SendAsync(new Attendance_Info_ACK(attendanceInfos, attendanceRewards, last));
		}

		public static void Handle_NewAttendanceGetUserInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new NewAttendanceGetUserInfoAck(last));
		}

		public static void Handle_AttendanceReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (attendanceReward(num, currentAccount.UserNum, out var Reward))
			{
				Client.SendAsync(new Attendance_Reward_ACK(0, num, Reward, last));
			}
			else
			{
				Client.SendAsync(new Attendance_Reward_ACK(1, 0, null, last));
			}
		}

		private static void attendanceInfo(int UserNum, out Dictionary<int, AttendanceInfo> attendanceInfos, out List<AttendanceReward> attendanceRewards)
		{
			attendanceInfos = new Dictionary<int, AttendanceInfo>();
			attendanceRewards = new List<AttendanceReward>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_attendanceInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					AttendanceInfo attendanceInfo = new AttendanceInfo
					{
						AttendanceKey = mySqlDataReader.GetInt32("fdAttendanceKey"),
						AttendanceType = mySqlDataReader.GetInt32("fdAttendanceType"),
						AttendanceRewardGroupKey = mySqlDataReader.GetInt32("fdAttendanceRewardGroupKey"),
						Start = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("fdStart")),
						EndS = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("fdEndS")),
						EndU = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("fdEndU")),
						AttendancePoint = mySqlDataReader.GetInt32("fdAttendancePoint")
					};
					attendanceInfos.Add(attendanceInfo.AttendanceKey, attendanceInfo);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					AttendanceReward item = new AttendanceReward
					{
						AttendanceRewardGroupKey = mySqlDataReader.GetInt32("fdAttendanceRewardGroupKey"),
						AttendanceRewardIndex = mySqlDataReader.GetInt32("fdAttendanceRewardIndex"),
						Item = mySqlDataReader.GetInt32("fdItem")
					};
					attendanceRewards.Add(item);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					UserAttendanceInfo userAttendanceInfo = new UserAttendanceInfo
					{
						AttendanceKey = mySqlDataReader.GetInt32("fdAttendanceKey"),
						AttendanceRewardIndex = mySqlDataReader.GetInt32("fdAttendanceRewardIndex"),
						LastDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("fdLastDate"))
					};
					if (attendanceInfos.ContainsKey(userAttendanceInfo.AttendanceKey))
					{
						attendanceInfos[userAttendanceInfo.AttendanceKey].userAttendanceInfos.AttendanceKey = userAttendanceInfo.AttendanceKey;
						attendanceInfos[userAttendanceInfo.AttendanceKey].userAttendanceInfos.AttendanceRewardIndex = userAttendanceInfo.AttendanceRewardIndex;
						attendanceInfos[userAttendanceInfo.AttendanceKey].userAttendanceInfos.LastDate = userAttendanceInfo.LastDate;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_attendanceInfo Error:{0}", ex.ToString());
			}
		}

		private static bool attendanceReward(int attendanceKey, int UserNum, out AttendanceReward Reward)
		{
			Reward = null;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_attendanceReward";
				mySqlCommand.Parameters.Add("attendanceKey", MySqlDbType.Int32).Value = attendanceKey;
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("is_debug", MySqlDbType.Int32).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					Reward = new AttendanceReward
					{
						AttendanceRewardGroupKey = mySqlDataReader.GetInt32("fdRewardGroupKey"),
						AttendanceRewardIndex = mySqlDataReader.GetInt32("fdRewardIndex"),
						Item = mySqlDataReader.GetInt32("fdItem")
					};
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_attendanceReward Error:{0}", ex.ToString());
			}
			return false;
		}
	}
}
