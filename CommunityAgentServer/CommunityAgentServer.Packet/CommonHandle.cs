using System;
using System.Collections.Generic;
using System.Data;
using CommunityAgentServer.Network.Connections;
using CommunityAgentServer.Packet.Send;
using CommunityAgentServer.Structuring;
using CommunityAgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace CommunityAgentServer.Packet
{
	public class CommonHandle
	{
		public static void Handle_0x02(ClientConnection Client, PacketReader reader)
		{
			int fixedLength = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength);
			reader.ReadByte();
			reader.ReadLEInt16();
			int fixedLength2 = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength2);
			Account account2 = (Client.CurrentAccount = new Account
			{
				NickName = text,
				Connection = Client,
				LastPingTime = Utility.CurrentTimeMilliseconds()
			});
			if (ClientConnection.CurrentAccounts.ContainsKey(text))
			{
				ClientConnection.CurrentAccounts.TryRemove(text, out var _);
			}
			ClientConnection.CurrentAccounts.TryAdd(text, Client.CurrentAccount);
			ClientConnection.DDOS_IP.TryRemove(Client.IP, out var _);
			Log.Information("CMA login opcode2 nick={0} ip={1}", text, Client.IP);
			Client.SendAsync(new NP_Hex("0300"));
		}

		public static void Handle_0x06(ClientConnection Client, PacketReader reader)
		{
			try
			{
				Account currentAccount = Client.CurrentAccount;
				int num = reader.ReadLEInt32();
				List<string> list = new List<string>();
				for (int i = 0; i < num; i++)
				{
					int fixedLength = reader.ReadLEInt16();
					string item = reader.ReadBig5StringSafe(fixedLength);
					list.Add(item);
				}
				short length = reader.ReadLEInt16();
				byte[] remain = reader.ReadByteArray(length);
				foreach (string item2 in list)
				{
					if (ClientConnection.CurrentAccounts.TryGetValue(item2, out var value))
					{
						value.Connection.SendAsync(new NP_0x07(currentAccount.NickName, remain));
					}
					else if (IsOfflineFriendPayload(remain))
					{
						Client.SendAsync(new NP_0x08(item2, remain));
					}
				}
			}
			catch (Exception ex)
			{
				string text = Utility.ByteArrayToString(reader.Buffer);
				Log.Error("Handle_0x06 Error:{0} packet:{1}", ex.Message, text.Substring(0, Math.Min(256, text.Length)));
			}
		}

		public static void Handle_0x07(ClientConnection Client, PacketReader reader)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			short length = reader.ReadLEInt16();
			byte[] remain = reader.ReadByteArray(length);
			if (remain != null && remain.Length >= 2 && remain[0] == 18 && remain[1] == 0)
			{
				Handle_CheckGift(Client, reader);
			}
			if (ClientConnection.CurrentAccounts.TryGetValue(text, out var value))
			{
				value.Connection.SendAsync(new NP_0x07(currentAccount.NickName, remain));
			}
			else if (IsOfflineFriendPayload(remain))
			{
				Client.SendAsync(new NP_0x08(text, remain));
			}
		}

		private static bool IsPresenceForward(byte[] remain)
		{
			if (remain == null || remain.Length < 2)
			{
				return false;
			}
			ushort inner = (ushort)(remain[0] | (remain[1] << 8));
			return inner == 7 || inner == 11 || inner == 33 || inner == 34;
		}

		/// <summary>
		/// Offline NP_0x08 is shown as a friend-request alert. Only allow known friend-add
		/// inners; drop presence / gift / state noise.
		/// </summary>
		private static bool IsOfflineFriendPayload(byte[] remain)
		{
			if (remain == null || remain.Length < 2)
			{
				return false;
			}
			if (IsPresenceForward(remain))
			{
				return false;
			}
			ushort inner = (ushort)(remain[0] | (remain[1] << 8));
			// 8 = friend request / add on this client family; 18 = CHECK_GIFT (not a friend alert).
			return inner == 8;
		}

		public static void Handle_CheckGift(ClientConnection Client, PacketReader reader)
		{
			_ = reader;
			Client.SendAsync(new NP_Hex("120000000000"));
		}

		public static void Handle_Ping(ClientConnection Client)
		{
			long num2 = (Client.CurrentAccount.LastPingTime = Utility.CurrentTimeMilliseconds());
		}

		public static void Handle_0x0E(ClientConnection Client, PacketReader reader)
		{
			Account currentAccount = Client.CurrentAccount;
			switch ((eCommunityAgentProfile)reader.ReadLEInt16())
			{
			case eCommunityAgentProfile.UNKNOWN_0_REQ:
				Client.SendAsync(new NP_Hex("0F00000000004B8ADA66B68B060070C1310000000000"));
				break;
			case eCommunityAgentProfile.SET_PROFILE_REQ:
			{
				reader.ReadLEInt32();
				reader.ReadByte();
				byte sex = reader.ReadByte();
				reader.ReadByte();
				byte location = reader.ReadByte();
				reader.ReadLEInt32();
				reader.ReadByte();
				int num = reader.ReadLEInt16();
				if (num > 2)
				{
					Client.SendAsync(new SetMyProfileFail());
					break;
				}
				string age = reader.ReadBig5StringSafe(num);
				reader.ReadByte();
				int fixedLength2 = reader.ReadLEInt16();
				string job = reader.ReadBig5StringSafe(fixedLength2);
				reader.ReadByte();
				int fixedLength3 = reader.ReadLEInt16();
				string hobby = reader.ReadBig5StringSafe(fixedLength3);
				if (ProfileSet(currentAccount, sex, age, location, job, hobby))
				{
					Client.SendAsync(new SetMyProfile());
				}
				else
				{
					Client.SendAsync(new SetMyProfileFail());
				}
				break;
			}
			case eCommunityAgentProfile.GET_MY_PROFILE_REQ:
				Client.SendAsync(new GetMyProfile(currentAccount));
				break;
			case eCommunityAgentProfile.GET_BY_NICK_REQ:
			{
				byte unk = reader.ReadByte();
				int fixedLength = reader.ReadLEInt16();
				string nickName = reader.ReadBig5StringSafe(fixedLength);
				Client.SendAsync(new GetProfileByNickName(nickName, unk));
				break;
			}
			}
		}

		private static bool ProfileSet(Account User, int Sex, string Age, int Location, string Job, string Hobby)
		{
			bool flag = false;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_profileSet";
			mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
			mySqlCommand.Parameters.Add("Sex", MySqlDbType.Int32).Value = Sex;
			mySqlCommand.Parameters.Add("Age", MySqlDbType.VarString).Value = Age;
			mySqlCommand.Parameters.Add("Location", MySqlDbType.Int32).Value = Location;
			mySqlCommand.Parameters.Add("Job", MySqlDbType.VarString).Value = Job;
			mySqlCommand.Parameters.Add("Hobby", MySqlDbType.VarString).Value = Hobby;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return (Convert.ToByte(mySqlDataReader["retval"]) == 0) ? true : false;
		}
	}
}
