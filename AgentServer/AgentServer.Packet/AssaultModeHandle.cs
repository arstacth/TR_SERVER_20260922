using System;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class AssaultModeHandle
	{
		public static void Handle_GetAssaultModeLimitAttackInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new AssaultModeAttackLimitInfo(last));
		}

		public static void Handle_GetAnubisPoint(ClientConnection Client, byte last)
		{
			Client.SendAsync(new AnubisUserPoint(0, last));
		}

		public static void Handle_GetAnubisOpenTime(ClientConnection Client, byte last)
		{
			Client.SendAsync(new AnubisOpenTime(last));
		}

		public static void Handle_GetDungeonRaidPoint(ClientConnection Client, byte last)
		{
			DungeonRaidGetUserPoint(Client.CurrentAccount.UserNum, out var point);
			Client.SendAsync(new AssaultRaidPoint(point, last));
		}

		public static void Handle_GetAssaultRaidOpenTime(ClientConnection Client, byte last)
		{
			Client.SendAsync(new AssaultRaidOpenTime(last));
		}

		private static bool DungeonRaidGetUserPoint(int UserNum, out int point)
		{
			point = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DungeonRaidGetUserPoint";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						point = Convert.ToInt32(mySqlDataReader["point"]);
						return true;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DungeonRaidGetUserPoint Error:{0}", ex.Message);
				return false;
			}
		}

		public static bool AssaultModeUseItem(int UserNum, int position, out int itemnum, out int itemcount)
		{
			itemnum = 0;
			itemcount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_assaultModeUseItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("position", MySqlDbType.Int32).Value = position;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						itemnum = Convert.ToInt32(mySqlDataReader["itemDescNum"]);
						itemcount = Convert.ToInt32(mySqlDataReader["itemCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_assaultModeUseItem Error:{0}", ex.Message);
				return false;
			}
		}

		public static void DungeonRaidAddPoint(int UserNum, int gotpoint)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_DungeonRaidAddPoint";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("gotPoint", MySqlDbType.Int32).Value = gotpoint;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_DungeonRaidAddPoint Error:{0}", ex.Message);
			}
		}
	}
}
