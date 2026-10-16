using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class TutorialChannel
	{
		public static void Handle_GetUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			TutorialChannel_GerUserInfo(Client.CurrentAccount.UserNum, out var infos);
			Client.SendAsync(new TutorialChannel_UserInfo(infos, last));
		}

		public static void Handle_RequestReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int level = reader.ReadLEInt32();
			int type = reader.ReadLEInt32();
			Log.Warning("TutorialChannel RequestReward user={0} type={1} level={2}",
				currentAccount.UserID, type, level);
			if (TutorialChannel_GiveReward(currentAccount.UserNum, type, level, out var exinfo))
			{
				currentAccount.TRNeedUpdateFromDB = true;
				currentAccount.EXPNeedUpdateFromDB = true;
				Log.Warning("TutorialChannel GiveReward OK user={0} rewards={1}",
					currentAccount.UserID, exinfo.Count);
				Client.SendAsync(new TutorialChannel_GiveRewardInfo(exinfo, last));
			}
			else
			{
				List<ExchangeItemInfo> peek = TutorialChannel_PeekRewards(type, level);
				if (peek.Count > 0)
				{
					// SP failed (often already-claimed / giveReward temp table) — still ACK rewards so UI completes.
					TryMarkTutorialClaimed(currentAccount.UserNum, type, level);
					currentAccount.TRNeedUpdateFromDB = true;
					currentAccount.EXPNeedUpdateFromDB = true;
					Log.Warning("TutorialChannel PeekReward fallback user={0} rewards={1}",
						currentAccount.UserID, peek.Count);
					Client.SendAsync(new TutorialChannel_GiveRewardInfo(peek, last));
				}
				else
				{
					Log.Warning("TutorialChannel GiveReward FAIL user={0} type={1} level={2}",
						currentAccount.UserID, type, level);
					Client.SendAsync(new TutorialChannel_GiveRewardInfo(new List<ExchangeItemInfo>(), last, error: 1));
				}
			}
		}

		private static void TutorialChannel_GerUserInfo(int UserNum, out List<TutorialChannelInfo> infos)
		{
			infos = new List<TutorialChannelInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_TutorialChannel_GerUserInfo";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TutorialChannelInfo item = new TutorialChannelInfo
					{
						Type = mySqlDataReader.GetInt32("fdType"),
						Level = mySqlDataReader.GetInt32("fdLevel")
					};
					infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_TutorialChannel_GerUserInfo Error: {0}", ex.Message);
			}
		}

		private static bool TutorialChannel_GiveReward(int UserNum, int Type, int Level, out List<ExchangeItemInfo> exinfo)
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
					mySqlCommand.CommandText = "usp_TutorialChannel_GiveReward";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pType", MySqlDbType.Int32).Value = Type;
					mySqlCommand.Parameters.Add("pLevel", MySqlDbType.Int32).Value = Level;
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
				Log.Error("usp_TutorialChannel_GiveReward Error: {0}", ex.Message);
				return false;
			}
		}

		private static List<ExchangeItemInfo> TutorialChannel_PeekRewards(int type, int level)
		{
			List<ExchangeItemInfo> list = new List<ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				int groupId = 0;
				// Live Thai schema: level is fdRewardLevel (fdLevel stays 0). Fall back to fdLevel.
				using (MySqlCommand groupCmd = new MySqlCommand(
					"SELECT fdRewardGroupID FROM essentutorialchannel_rewardinfo WHERE fdType=@t AND (fdRewardLevel=@l OR (IFNULL(fdRewardLevel,0)=0 AND fdLevel=@l)) LIMIT 1",
					mySqlConnection))
				{
					groupCmd.Parameters.AddWithValue("@l", level);
					groupCmd.Parameters.AddWithValue("@t", type);
					object o = groupCmd.ExecuteScalar();
					if (o != null && o != DBNull.Value)
					{
						groupId = Convert.ToInt32(o);
					}
				}
				if (groupId <= 0)
				{
					return list;
				}
				using MySqlCommand itemCmd = new MySqlCommand(
					"SELECT fdRewardType, fdRewardItem, fdRewardCount FROM essenrewardgroupdetail WHERE fdRewardGroupID=@g",
					mySqlConnection);
				itemCmd.Parameters.AddWithValue("@g", groupId);
				using MySqlDataReader r = itemCmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new ExchangeItemInfo
					{
						type = r.GetInt32(0),
						id = r.GetInt32(1),
						count = r.GetInt32(2)
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("TutorialChannel_PeekRewards Error: {0}", ex.Message);
			}
			return list;
		}

		private static void TryMarkTutorialClaimed(int userNum, int type, int level)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"INSERT IGNORE INTO UserTutorialChannelInfo (fdUserNum, fdType, fdLevel) VALUES (@u, @t, @l)",
					conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				cmd.Parameters.AddWithValue("@t", type);
				cmd.Parameters.AddWithValue("@l", level);
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Warning("TryMarkTutorialClaimed: {0}", ex.Message);
			}
		}
	}
}
