using System;
using System.Data;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using Serilog;

namespace RoomServer.Packet
{
	public class TowerEvent
	{
		public static void Handle_EnterEvent(Account User, PacketReader reader, byte last)
		{
			reader.ReadByte();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null && room.RoomKindID == 74)
			{
				if (!User.TowerEventJoined && TowerEventJoin(User.UserNum, out var IsUseItem, out var FreePlayCount, out var TicketPlayCount))
				{
					User.TowerEventJoined = true;
					User.SendAsync(new TowerEvent_Enter_ACK(1, FreePlayCount, TicketPlayCount, IsUseItem, last));
				}
				else
				{
					User.SendAsync(new TowerEvent_Enter_ACK(6, 0, 0, IsUseItem: false, last));
				}
			}
		}

		public static void Handle_GetBox(Account User, PacketReader reader, byte last)
		{
			int boxID = reader.ReadLEInt32();
			int boxType = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.TowerEventJoined && room.GetBox(boxID, boxType))
			{
				TowerEvent_GiveReward(User.UserNum, boxType, out var ResultItem);
				User.SendAsync(new TowerEvent_GetBox_ACK(boxID, boxType, ResultItem, 1, last));
			}
		}

		public static void Handle_GetBox2(Account User, PacketReader reader, byte last)
		{
			int boxID = reader.ReadLEInt32();
			int boxType = reader.ReadLEInt32();
			short fixedLength = reader.ReadLEInt16();
			string answer = reader.ReadBig5StringSafe(fixedLength);
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.TowerEventJoined && room.GetBox2(boxID, boxType, answer))
			{
				TowerEvent_GiveReward(User.UserNum, boxType, out var ResultItem);
				User.SendAsync(new TowerEvent_GetBox_ACK(boxID, boxType, ResultItem, 1, last));
			}
			else
			{
				User.SendAsync(new TowerEvent_GetBox_ACK(boxID, boxType, 0, 8, last));
			}
		}

		public static void Handle_GiveUP(Account User, byte last)
		{
			if (User.TowerEventJoined)
			{
				User.TowerEventJoined = false;
				User.SendAsync(new TowerEvent_GiveUP_ACK(last));
			}
		}

		public static void Handle_UserGetItemInfo(Account User, PacketReader reader, byte last)
		{
			reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			User.SendAsync(new TowerEvent_UserGetItemInfo_ACK(last));
			User.SendAsync(new TowerEvent_RemainBox2_ACK(room.TowerEvent_BoxList, last));
		}

		private static bool TowerEventJoin(int UserNum, out bool IsUseItem, out int FreePlayCount, out int TicketPlayCount)
		{
			IsUseItem = false;
			FreePlayCount = 0;
			TicketPlayCount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TowerEventJoin";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("StartTime", MySqlDbType.DateTime).Value = ServerStatus.TowerEventStartTime;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						IsUseItem = Convert.ToBoolean(mySqlDataReader["TicketResult"]);
						FreePlayCount = Convert.ToInt32(mySqlDataReader["FreePlayCount"]);
						TicketPlayCount = Convert.ToInt32(mySqlDataReader["TicketPlayCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TowerEventJoin Error: {0}", ex.Message);
				return false;
			}
		}

		private static void TowerEvent_GiveReward(int UserNum, int BoxType, out int ResultItem)
		{
			ResultItem = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_TowerEvent_GiveReward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("BoxType", MySqlDbType.Int32).Value = BoxType;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					ResultItem = mySqlDataReader.GetInt32("GiveItem");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_TowerEvent_GiveReward Error: {0}", ex.Message);
			}
		}
	}
}
