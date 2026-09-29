using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Database;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class AvatarLock
	{
		public static void Handle_AvatarLock_Load(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (AvatarLock_Load(currentAccount))
			{
				Client.SendAsync(new AvatarLock_Load_New(currentAccount, last));
				if (currentAccount.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_AVATAR_LOCK(currentAccount, last), room.RoomServerID);
				}
			}
		}

		public static void Handle_AvatarLock_Save(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			bool flag = reader.ReadBoolean();
			List<int> itemList = new List<int>();
			List<int> itemList2 = new List<int>();
			bool flag2 = false;
			CAvatarLock cAvatarLock = new CAvatarLock();
			cAvatarLock.setFromAdvancedAvatarInfo(currentAccount.advancedAvatarInfo);
			if (flag)
			{
				if (!cAvatarLock.getAvatarItemList(AVATAR.AVATAR_REAL, out itemList) || !cAvatarLock.getAvatarItemList(AVATAR.AVATAR_COSTUME, out itemList2))
				{
					return;
				}
			}
			else if (!cAvatarLock.getAvatarItemList(AVATAR.AVATAR_REAL, out itemList2))
			{
				return;
			}
			flag2 = cAvatarLock.isUseCostume;
			if (AvatarLock_Save(currentAccount, itemList, itemList2, flag2, flag, out var m_lookItems))
			{
				if (currentAccount.isInRoom(out var room))
				{
					byte[] packet = new AvatarLock_Save_New(currentAccount, m_lookItems, last).ToArray();
					room.BroadcastToMe(currentAccount.Session, packet);
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_AVATAR_LOCK(currentAccount, last), room.RoomServerID);
				}
				else
				{
					Client.SendAsync(new AvatarLock_Save_New(currentAccount, m_lookItems, last));
				}
			}
		}

		public static bool AvatarLock_Load(Account User)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			bool bUseCostume = false;
			bool flag = false;
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_AvatarLock_Load"))
				{
					mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						list.Add(mySqlCommandHelper.GetInt("realItemNum"));
					}
					mySqlCommandHelper.NextResult();
					while (mySqlCommandHelper.HasResult())
					{
						list2.Add(mySqlCommandHelper.GetInt("cosItemNum"));
					}
					mySqlCommandHelper.NextResult();
					if (mySqlCommandHelper.HasResult())
					{
						bUseCostume = mySqlCommandHelper.GetBoolean("isCostume");
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_AvatarLock_Load error: {0}", ex.Message);
			}
			if (flag)
			{
				User.avatarLock = new CAvatarLock(list, list2, bUseCostume);
			}
			return flag;
		}

		public static bool UserInfo_AvatarLock_Load(int userNum, out List<int> AvatarLock_RealItem, out List<int> AvatarLock_CostumeItem)
		{
			AvatarLock_RealItem = new List<int>();
			AvatarLock_CostumeItem = new List<int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_AvatarLock_Load";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					AvatarLock_RealItem.Add(mySqlDataReader.GetInt32("realItemNum"));
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					AvatarLock_CostumeItem.Add(mySqlDataReader.GetInt32("cosItemNum"));
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_AvatarLock_Load error: {0}", ex.Message);
				return false;
			}
		}

		private static bool AvatarLock_Save(Account User, List<int> avatarItems, List<int> costumeItems, bool bCostume, bool bLock, out List<int> m_lookItems)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			foreach (int avatarItem in avatarItems)
			{
				text += $"{avatarItem},";
			}
			foreach (int costumeItem in costumeItems)
			{
				text2 += $"{costumeItem},";
			}
			m_lookItems = new List<int>();
			bool flag = false;
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_AvatarLock_Save"))
				{
					mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
					mySqlCommandHelper.AddParamVarString("realItems", text);
					mySqlCommandHelper.AddParamVarString("costumeItems", text2);
					mySqlCommandHelper.AddParamBoolean("bCostume", bCostume);
					mySqlCommandHelper.AddParamBoolean("bLock", bLock);
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						m_lookItems.Add(mySqlCommandHelper.GetInt("cosItemNum"));
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_AvatarLock_Save error: {0}", ex.Message);
			}
			if (flag)
			{
				User.avatarLock = new CAvatarLock(avatarItems, costumeItems, bCostume);
			}
			return flag;
		}
	}
}
