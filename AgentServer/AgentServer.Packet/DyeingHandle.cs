using System;
using System.Data;
using AgentServer.Function;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class DyeingHandle
	{
		public static void Handle_ItemDyeing(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte b = reader.ReadByte();
			byte[] color = reader.ReadByteArray(3);
			byte[] color2 = reader.ReadByteArray(3);
			byte[] color3 = reader.ReadByteArray(3);
			byte b2 = reader.ReadByte();
			UserItemDyeing userItemDyeing = new UserItemDyeing
			{
				DyeingPart = b,
				Color1 = color,
				Color2 = color2,
				Color3 = color3,
				Type = b2
			};
			UserItemDyeing value;
			bool num2 = currentAccount.DyedItemList.TryGetValue(b2, num, out value);
			bool flag = true;
			if (num2)
			{
				flag = FlagsHelper.IsSet(value.DyeingPart, b);
			}
			if (num2 && !flag)
			{
				FlagsHelper.Set(ref value.DyeingPart, b);
			}
			if (num2)
			{
				userItemDyeing.DyeingPart = value.DyeingPart;
			}
			if (ItemDyeing(currentAccount.UserNum, num, userItemDyeing, 0, needUpdatePart: true) == 0)
			{
				currentAccount.DyedItemList[b2][num] = userItemDyeing;
				ItemHandle.getCurrentAvatarInfo(currentAccount, bRequestNickName: false, last);
				currentAccount.activeItem.useItem(78309);
				Client.SendAsync(new DyeItemOK(num, userItemDyeing, last));
			}
			else
			{
				Client.SendAsync(new DyeItemOK(num, null, last, 1));
			}
		}

		public static void Handle_ItemDyeingRestore(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte flag = reader.ReadByte();
			byte[] color = reader.ReadByteArray(3);
			byte[] color2 = reader.ReadByteArray(3);
			byte[] color3 = reader.ReadByteArray(3);
			byte b = reader.ReadByte();
			if (currentAccount.DyedItemList.TryGetValue(b, num, out var value))
			{
				if (!FlagsHelper.IsSet(value.DyeingPart, flag))
				{
					Client.SendAsync(new DyeItemRestoreOK(num, null, last, 283));
					return;
				}
				FlagsHelper.Unset(ref value.DyeingPart, flag);
				UserItemDyeing userItemDyeing = new UserItemDyeing
				{
					DyeingPart = value.DyeingPart,
					Color1 = color,
					Color2 = color2,
					Color3 = color3,
					Type = b
				};
				if (ItemDyeing(currentAccount.UserNum, num, userItemDyeing, 1, needUpdatePart: true) == 0)
				{
					currentAccount.DyedItemList[b][num] = userItemDyeing;
					ItemHandle.getCurrentAvatarInfo(currentAccount, bRequestNickName: false, last);
					Client.SendAsync(new DyeItemRestoreOK(num, userItemDyeing, last));
				}
				else
				{
					Client.SendAsync(new DyeItemRestoreOK(num, null, last, 1));
				}
			}
			else
			{
				Client.SendAsync(new DyeItemRestoreOK(num, null, last, 1));
			}
		}

		public static void GetUserItemDyeingInfo(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemdyeing_getUserInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdItemDescNum"]);
					byte b = Convert.ToByte(mySqlDataReader["fdType"]);
					UserItemDyeing value = new UserItemDyeing
					{
						DyeingPart = Convert.ToByte(mySqlDataReader["fdPart"]),
						Color1 = Utility.StringToByteArray(mySqlDataReader["fdColor1"].ToString()),
						Color2 = Utility.StringToByteArray(mySqlDataReader["fdColor2"].ToString()),
						Color3 = Utility.StringToByteArray(mySqlDataReader["fdColor3"].ToString()),
						Type = b
					};
					User.DyedItemList[b][key] = value;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemdyeing_getUserInfo error: {0}", ex.Message);
			}
		}

		private static int ItemDyeing(int UserNum, int itemnum, UserItemDyeing info, byte isRemove, bool needUpdatePart)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemdyeing_dye";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("DyeItemNum", MySqlDbType.Int32).Value = itemnum;
				mySqlCommand.Parameters.Add("Part", MySqlDbType.Int16).Value = info.DyeingPart;
				mySqlCommand.Parameters.Add("Color1", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(info.Color1).Substring(0, 6);
				mySqlCommand.Parameters.Add("Color2", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(info.Color2).Substring(0, 6);
				mySqlCommand.Parameters.Add("Color3", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(info.Color3).Substring(0, 6);
				mySqlCommand.Parameters.Add("Type", MySqlDbType.Int16).Value = info.Type;
				mySqlCommand.Parameters.Add("isUpdatePart", MySqlDbType.Int16).Value = needUpdatePart;
				mySqlCommand.Parameters.Add("isRemove", MySqlDbType.Int16).Value = isRemove;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				return mySqlDataReader.GetInt32("ret");
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemdyeing_dye error: {0}", ex.Message);
				return 5;
			}
		}
	}
}
