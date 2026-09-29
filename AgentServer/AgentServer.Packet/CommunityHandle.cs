using System;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet
{
	public class CommunityHandle
	{
		public static void Handle_AddFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			string invitationmessage = string.Empty;
			if (num > 0)
			{
				invitationmessage = reader.ReadBig5StringSafe(num);
			}
			short num2 = reader.ReadLEInt16();
			if (currentAccount.NickName != text)
			{
				AddFriendCheck(currentAccount, text, invitationmessage, num2, out var ret);
				if (ret == 0)
				{
					Client.SendAsync(new AddFriendSuccess(currentAccount, text, num2, last));
				}
				else
				{
					Client.SendAsync(new AddFriendFail(currentAccount, text, last));
				}
			}
			else
			{
				Client.SendAsync(new AddFriendFail(currentAccount, text, last));
			}
		}

		public static void Handle_DeleteFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			if (currentAccount.NickName != text)
			{
				DeleteFriend(currentAccount, text, out var ret);
				if (ret == 0)
				{
					Client.SendAsync(new DeleteFriendOK(text, last));
				}
				else
				{
					Client.SendAsync(new DeleteFriendFail(text, ret, last));
				}
			}
		}

		public static void Handle_CancelAddFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			CancleAddFriend(currentAccount, text, out var ret);
			if (ret == 0)
			{
				Client.SendAsync(new CancelAddFriendOK(currentAccount, text, last));
			}
		}

		public static void Handle_DeclineFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			DeclineAddFriend(currentAccount, text, out var ret);
			if (ret == 0)
			{
				Client.SendAsync(new DeclineAddFriendOK(currentAccount, text, last));
			}
		}

		public static void Handle_AcceptFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			AcceptFriend(currentAccount, text, out var ret);
			if (ret == 0)
			{
				Client.SendAsync(new AcceptFriendOK(currentAccount, text, last));
			}
		}

		public static void Handle_GetFriendListAccepted(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Client.SendAsync(new GetAcceptedFriendList_7406(currentAccount, last));
			Client.SendAsync(new GetWaitAcceptFriendList_7407(currentAccount, last));
		}

		public static void Handle_GetFriendGroup(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			switch (reader.ReadByte())
			{
			case 0:
				Client.SendAsync(new GetFriendGroupList_7420(currentAccount, last));
				break;
			case 1:
			{
				int fixedLength2 = reader.ReadLEInt16();
				string text2 = reader.ReadBig5StringSafe(fixedLength2);
				if (text2 != "DEFAULT")
				{
					Client.SendAsync(new AddFriendGroup_742001(currentAccount, text2, last));
				}
				break;
			}
			case 2:
			{
				short num2 = reader.ReadLEInt16();
				byte isfolding = reader.ReadByte();
				int fixedLength = reader.ReadLEInt16();
				string text = reader.ReadBig5StringSafe(fixedLength);
				if (text != "DEFAULT" && num2 != 0)
				{
					Client.SendAsync(new ModifytFriendGroup_742002(currentAccount, num2, text, isfolding, last));
				}
				break;
			}
			case 3:
			{
				short num = reader.ReadLEInt16();
				if (num != 0)
				{
					Client.SendAsync(new DelFriendGroup_742003(currentAccount, num, last));
				}
				break;
			}
			}
		}

		public static void Handle_GroupMoveMember(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short groupnum = reader.ReadLEInt16();
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			Client.SendAsync(new GroupMoveMember(currentAccount, groupnum, nickname, last));
		}

		public static void Handle_BlockFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			BlockFriend(currentAccount, nickname, out var ret);
			if (ret == 0)
			{
				Client.SendAsync(new BlockFriendOK(currentAccount, nickname, last));
			}
		}

		public static void Handle_UnBlockFriend(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			UnBlockFriend(currentAccount, nickname, out var ret);
			if (ret == 0)
			{
				Client.SendAsync(new UnBlockFriendOK(currentAccount, nickname, last));
			}
		}

		public static void Handle_GetRequestedToMe(ClientConnection Client, byte last)
		{
			GetRequestedToMe_741E packet = new GetRequestedToMe_741E(Client.CurrentAccount, last);
			// Official / KR only emit ACK when a real pending request exists.
			if (packet.HasRequest)
			{
				Client.SendAsync(packet);
			}
		}

		public static void Handle_CheckGift(ClientConnection Client, byte last)
		{
			Client.SendAsync(new UnknownCMPacket1(last, eCommunityProtocol.CHECK_GIFT_REQ));
		}

		public static void Handle_GetUserAlarmInfo(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Client.SendAsync(new GetUserAlarmInfo(currentAccount, last));
		}

		public static void Handle_0x7426(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownCMPacket1(last));
		}

		public static void Handle_Community46(ClientConnection Client, byte last)
		{
			_ = last;
			Client.SendAsync(new UnknownCMPacket1(last, eCommunityProtocol.COMMUNITY_46_ACK));
		}

		public static void Handle_GetGuildMemberList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte type = reader.ReadByte();
			if (GuildHandle.GetGuildMemberInfo(currentAccount.UserNum, out var memberinfos))
			{
				Client.SendAsync(new CM_GetGuildMemberACK(type, memberinfos, last));
			}
			if (GuildHandle.GetGuildUserInfo(currentAccount.UserNum, out var userinfo))
			{
				currentAccount.GuildUserInfo = userinfo;
			}
		}

		public static void Handle_UpdateGuildMemberList(ClientConnection Client, byte last)
		{
			if (GuildHandle.GetGuildMemberInfo(Client.CurrentAccount.UserNum, out var memberinfos))
			{
				Client.SendAsync(new CM_UpdateGuildMemberACK(memberinfos, last));
			}
		}

		public static void Handle_ModifyMemo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int fixedLength2 = reader.ReadLEInt16();
			string text2 = reader.ReadBig5StringSafe(fixedLength2);
			if (text2.Length <= 50)
			{
				ModifyMemo(currentAccount.UserNum, text, text2);
				Client.SendAsync(new ModifyMemoOK_ACK(text, text2, last));
			}
		}

		private static void AddFriendCheck(Account User, string requestednickname, string invitationmessage, short groupNum, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_addfriend";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("requestednickname", MySqlDbType.VarString).Value = requestednickname;
			mySqlCommand.Parameters.Add("invitationmessage", MySqlDbType.VarString).Value = invitationmessage;
			mySqlCommand.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupNum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void CancleAddFriend(Account User, string requestednickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_cancelAddFriend";
			mySqlCommand.Parameters.Add("myusernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("requestednickname", MySqlDbType.VarString).Value = requestednickname;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void DeclineAddFriend(Account User, string requestnickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_declinefriend";
			mySqlCommand.Parameters.Add("requestNickname", MySqlDbType.VarString).Value = requestnickname;
			mySqlCommand.Parameters.Add("myusernum", MySqlDbType.Int32).Value = User.UserNum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void AcceptFriend(Account User, string requestnickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_acceptFriend";
			mySqlCommand.Parameters.Add("requestNickname", MySqlDbType.VarString).Value = requestnickname;
			mySqlCommand.Parameters.Add("requestedusernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("friendType", MySqlDbType.Int32).Value = 0;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void BlockFriend(Account User, string nickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_blockFriend";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void UnBlockFriend(Account User, string nickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_unblockFriend";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void DeleteFriend(Account User, string nickname, out byte ret)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_deleteFriend";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			ret = Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static void ModifyMemo(int UserNum, string memberNickname, string memo)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_modifyMemo";
			mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
			mySqlCommand.Parameters.Add("memberNickname", MySqlDbType.VarString).Value = memberNickname;
			mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
			mySqlCommand.ExecuteNonQuery();
		}
	}
}
