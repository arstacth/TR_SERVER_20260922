using System;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using NetMsg.Room;
using Serilog;

namespace AgentServer.Packet
{
	public class TowerEvent
	{
		public static void Handle_GetUserJoinInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			TowerEvent_GetTodayJoinData(Client.CurrentAccount.UserNum, out var FreePlayCount, out var TicketPlayCount);
			Client.SendAsync(new TowerEvent_UserJoinInfo_ACK(FreePlayCount, TicketPlayCount, last));
		}

		public static void Handle_EnterEvent(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int size = reader.Size;
			byte[] array = new byte[size];
			Buffer.BlockCopy(reader.Buffer, 0, array, 0, size);
			ServerStatus.ToRoomServer(new RM_Packet
			{
				Session = currentAccount.Session,
				data = array
			}, currentAccount.RoomServerID);
		}

		public static void Handle_GiveUP(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int size = reader.Size;
			byte[] array = new byte[size];
			Buffer.BlockCopy(reader.Buffer, 0, array, 0, size);
			ServerStatus.ToRoomServer(new RM_Packet
			{
				Session = currentAccount.Session,
				data = array
			}, currentAccount.RoomServerID);
		}

		public static void Handle_UserGetItemInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int size = reader.Size;
			byte[] array = new byte[size];
			Buffer.BlockCopy(reader.Buffer, 0, array, 0, size);
			ServerStatus.ToRoomServer(new RM_Packet
			{
				Session = currentAccount.Session,
				data = array
			}, currentAccount.RoomServerID);
		}

		private static void TowerEvent_GetTodayJoinData(int UserNum, out int FreePlayCount, out int TicketPlayCount)
		{
			FreePlayCount = 0;
			TicketPlayCount = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_TowerEvent_GetTodayJoinData";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					FreePlayCount = Convert.ToInt32(mySqlDataReader["FreePlayCount"]);
					TicketPlayCount = Convert.ToInt32(mySqlDataReader["TicketPlayCount"]);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_TowerEvent_GetTodayJoinData Error: {0}", ex.Message);
			}
		}
	}
}
