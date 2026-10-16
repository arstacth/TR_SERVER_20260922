using System;
using System.Data;
using System.Text.RegularExpressions;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet
{
	public class MessageBoxHandle
	{
		public static void Handle_SearchNickName(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			bool flag = false;
			if (!new Regex("[~`!@#$%^&*()+=|\\\\{}':;.,<>/?[\\]\"_-]").IsMatch(text))
			{
				flag = SearchNickName(text);
			}
			if (flag)
			{
				Client.SendAsync(new SearchNickName(text, last));
			}
			else
			{
				Client.SendAsync(new SearchNickNameFail(last));
			}
		}

		public static void Handle_SendMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.Offset += 4;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			string msg = reader.ReadBig5StringSafeNL(num);
			if (num >= 10 && num <= 200)
			{
				if (SendMessage(currentAccount, text, msg, out var messageNum, out var sendDateTime))
				{
					Client.SendAsync(new SendMessageOK(currentAccount.NickName, text, messageNum, sendDateTime, msg, last));
				}
				else
				{
					Client.SendAsync(new SendMessageFail(last));
				}
			}
		}

		public static void Handle_GetReceiveList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			short iPage = reader.ReadLEInt16();
			short iList = reader.ReadLEInt16();
			short iRequestCount = reader.ReadLEInt16();
			switch (num)
			{
			case 1:
			case 3:
				Client.SendAsync(new GetReceiveListOK(num, currentAccount.UserNum, iPage, iList, iRequestCount, last));
				break;
			case 2:
				Client.SendAsync(new GetSendListOK(num, currentAccount.UserNum, iPage, iList, iRequestCount, last));
				break;
			}
		}

		public static void Handle_ReadMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			long messageNum = reader.ReadLEInt64();
			ReadMessage(currentAccount.UserNum, messageNum);
			Client.SendAsync(new ReadMessageOK(messageNum, last));
		}

		public static void Handle_ReportMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			long messageNum = reader.ReadLEInt64();
			ReportMessage(currentAccount.UserNum, messageNum);
			Client.SendAsync(new ReportMessageOK(messageNum, last));
		}

		public static void Handle_DeleteMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte isAll = reader.ReadByte();
			int num2 = reader.ReadLEInt32();
			string text = string.Empty;
			for (int i = 0; i < num2; i++)
			{
				long num3 = reader.ReadLEInt64();
				text += $"{num3},";
			}
			switch (num)
			{
			case 1:
			case 3:
				if (DeleteReceiveMessage(currentAccount.UserNum, num, isAll, text))
				{
					Client.SendAsync(new DeleteReceiveMessageOK(num, isAll, text, last));
				}
				break;
			case 2:
				if (DeleteSendMessage(currentAccount.UserNum, num, isAll, text))
				{
					Client.SendAsync(new DeleteReceiveMessageOK(num, isAll, text, last));
				}
				break;
			}
		}

		public static void Handle_KeepMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			long messageNum = reader.ReadLEInt64();
			if (KeepMessage(currentAccount.UserNum, messageNum))
			{
				Client.SendAsync(new KeepMessageOK(messageNum, last));
			}
		}

		public static void Handle_GetOption(ClientConnection Client, byte last)
		{
			GetOption(Client.CurrentAccount.UserNum, out var option);
			Client.SendAsync(new GetOption(option, last));
		}

		public static void Handle_OptionChange(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short num = reader.ReadLEInt16();
			if (num < 0 || num > 2)
			{
				num = 0;
			}
			if (ChangeOption(currentAccount.UserNum, num))
			{
				Client.SendAsync(new GetOption(num, last));
			}
		}

		private static bool SearchNickName(string NickName)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxSearchNickName";
				mySqlCommand.Parameters.Add("pSearchNickName", MySqlDbType.VarString).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					return true;
				}
			}
			return false;
		}

		private static bool SendMessage(Account User, string NickName, string Msg, out long messageNum, out long sendDateTime)
		{
			messageNum = 0L;
			sendDateTime = 0L;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxSendMessage";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pSendNickname", MySqlDbType.VarString).Value = User.NickName;
				mySqlCommand.Parameters.Add("pRecvNickname", MySqlDbType.VarString).Value = NickName;
				mySqlCommand.Parameters.Add("pMessageKind", MySqlDbType.Int32).Value = 0;
				mySqlCommand.Parameters.Add("isSystemMessage", MySqlDbType.Int32).Value = 0;
				mySqlCommand.Parameters.Add("pMessage", MySqlDbType.VarString).Value = Msg;
				mySqlCommand.Parameters.Add("messageBoxNum", MySqlDbType.Int64).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					messageNum = Convert.ToInt64(mySqlDataReader["messageNum"]);
					sendDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["sendDateTime"]));
					return true;
				}
			}
			return false;
		}

		private static bool ReadMessage(int UserNum, long messageNum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxReadMessage";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("messageNum", MySqlDbType.Int64).Value = messageNum;
				mySqlCommand.ExecuteNonQuery();
			}
			return true;
		}

		private static bool ReportMessage(int UserNum, long messageNum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxReportMessage";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("messageNum", MySqlDbType.Int64).Value = messageNum;
				mySqlCommand.ExecuteNonQuery();
			}
			return true;
		}

		private static bool DeleteReceiveMessage(int UserNum, int messageType, byte isAll, string messagenum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxDeleteReceiveMessage";
				mySqlCommand.Parameters.Add("messageType", MySqlDbType.Int32).Value = messageType;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("isAll", MySqlDbType.Int16).Value = isAll;
				mySqlCommand.Parameters.Add("messagenum", MySqlDbType.VarString).Value = messagenum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool DeleteSendMessage(int UserNum, int messageType, byte isAll, string messagenum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxDeleteSendMessage";
				mySqlCommand.Parameters.Add("messageType", MySqlDbType.Int32).Value = messageType;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("isAll", MySqlDbType.Int16).Value = isAll;
				mySqlCommand.Parameters.Add("messagenum", MySqlDbType.VarString).Value = messagenum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool KeepMessage(int UserNum, long messageNum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxKeepMessage";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("messageNum", MySqlDbType.Int64).Value = messageNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static void GetOption(int UserNum, out int option)
		{
			option = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_messageBoxOption";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			option = Convert.ToInt32(mySqlDataReader["messageBoxOption"]);
		}

		private static bool ChangeOption(int UserNum, short option)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxOptionChange";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("optionType", MySqlDbType.Int16).Value = option;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
