using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AgentServer.Database;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class FirstLoginHandle
	{
		public static void Handle_SelectStartCharacter(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int charid = reader.ReadLEInt16();
			Log.Information("SELECT_START_CHARACTER user={0} charKind={1}", currentAccount.UserID, charid);
			currentAccount.AvatarItemDyeing.AddRange(Enumerable.Repeat(new UserItemDyeing(), 24));
			if (selectStartCharacter(currentAccount, charid, last))
			{
				Client.SendAsync(new FirstLoginMakeStartCharacterOK(last));
				ItemHandle.getCurrentAvatarInfo(currentAccount, bRequestNickName: true, last);
			}
			else
			{
				Client.SendAsync(new FirstLoginMakeStartCharacterFail(last));
			}
		}

		public static void Handle_SetNewNickName(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			try
			{
				bool flag = false;
				int result = 0;
				int fixedLength = reader.ReadLEInt16();
				string text = reader.ReadBig5StringSafe(fixedLength);
				Regex regex = new Regex("^[0-9A-Za-z\\u0E00-\\u0E7F\\u4E00-\\u9FFF]+$");
				int byteCount = Encoding.Default.GetByteCount(text);
				if (byteCount < 4 || byteCount > 12 || !regex.IsMatch(text))
				{
					flag = false;
					result = 2;
				}
				else
				{
					reader.Clear();
					flag = SetNewNickNameCheck(currentAccount, text, out result);
				}
				if (flag && result == 0)
				{
					Client.SendAsync(new FirstLoginSetNewNickNameOK(currentAccount.NickName, last));
					Client.SendAsync(new LoginGetNickName_0X1C(currentAccount, last));
					Client.SendAsync(new FirstLogin_CreateFarm_GetMyFarmInfo(currentAccount, last));
				}
				else if (!flag && result == 1)
				{
					Client.SendAsync(new FirstLoginSetNewNickNameFail_0X4C(last));
				}
				else
				{
					Client.SendAsync(new FirstLoginSetNewNickNameFail_0x63(text, last));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static bool SetNewNickNameCheck(Account User, string NickName, out int result)
		{
			result = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_setNickname";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				result = Convert.ToInt32(mySqlDataReader["result"]);
				if (result == 0)
				{
					User.noNickName = false;
					User.NickName = NickName;
					return true;
				}
			}
			return false;
		}

		private static bool selectStartCharacter(Account User, int charid, byte last)
		{
			List<NetItemInfo> list = new List<NetItemInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_selectStartCharacter");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("charKind", charid);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					NetItemInfo netItemInfo = new NetItemInfo();
					netItemInfo.clear();
					netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
					netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
					netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
					netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
					netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
					if (mySqlCommandHelper.IsDBNull("expireTime"))
					{
						netItemInfo.m_bHasExpireTime = false;
						netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					}
					else
					{
						netItemInfo.m_bHasExpireTime = true;
						netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
						netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					}
					netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
					list.Add(netItemInfo);
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_selectStartCharacter Error: {0}", ex.ToString());
			}
			if (flag)
			{
				User.onRecvActiveFuncItem(flag, list, bEquipmentItem: false);
			}
			return flag;
		}
	}
}
