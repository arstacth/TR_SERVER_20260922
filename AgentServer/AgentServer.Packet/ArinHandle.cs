using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class ArinHandle
	{
		public static void Handle_ItemStrengthen_StrengthenSlot(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			short num2 = reader.ReadLEInt16();
			reader.ReadLEInt32();
			bool flag = true;
			bool flag2 = true;
			int err = 0;
			short remainStrengthenCount = 0;
			ItemStrengthen value;
			if (!currentAccount.UserItemStrengthen.ContainsKey(num))
			{
				err = 3;
				flag = false;
			}
			else if (currentAccount.UserItemStrengthen.TryGetValue(num, out value) && value.strengthenCount <= 0)
			{
				err = 6;
				flag = false;
			}
			else if (currentAccount.UserItemStrengthenSlotGroup.ContainsKey(num, (byte)num2))
			{
				err = 8;
				flag = false;
			}
			else
			{
				flag2 = ItemStrengthen_StrengthenSlot(currentAccount, num, num2, failed: false, out remainStrengthenCount);
			}
			if (flag && flag2)
			{
				GroupItemStrengthenAttr(currentAccount);
				Client.SendAsync(new ItemStrengthen_StrengthenSlot_ACK(currentAccount, num, num2, remainStrengthenCount, last));
			}
			else
			{
				Client.SendAsync(new ItemStrengthen_StrengthenSlot_Failed_ACK(err, num, num2, last));
			}
		}

		public static void Handle_ItemStrengthen_CleanSlot(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			int cleanItemNum = reader.ReadLEInt32();
			short cleanType = reader.ReadLEInt16();
			bool flag = true;
			bool flag2 = true;
			int err = 0;
			int maxMySlotNum = 0;
			if (!currentAccount.UserItemStrengthen.ContainsKey(num))
			{
				err = 3;
				flag = false;
			}
			else if (!currentAccount.UserItemStrengthenSlotGroup.ContainsKey(num, (byte)num2))
			{
				err = 6;
				flag = false;
			}
			else
			{
				flag2 = ItemStrengthen_CleanSlot(currentAccount, num, cleanItemNum, num2, cleanType, out maxMySlotNum);
			}
			if (flag && flag2)
			{
				GroupItemStrengthenAttr(currentAccount);
				Client.SendAsync(new ItemStrengthen_CleanSlot_ACK(num, (short)num2, maxMySlotNum, currentAccount.TR, last));
			}
			else
			{
				Client.SendAsync(new ItemStrengthen_CleanSlot_Failed_ACK(err, num, (short)num2, last));
			}
		}

		public static void Handle_IncreaseStrengthenCount(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.ReadLEInt32();
			int increaseItemNum = reader.ReadLEInt32();
			if (IncreaseStrengthenCount(currentAccount, itemNum, increaseItemNum, out var strengthenCount, out var remainItemCount))
			{
				itemStrengthen_getUserItem(currentAccount, itemNum);
				Client.SendAsync(new IncreaseStrengthenCount_ACK(itemNum, strengthenCount, increaseItemNum, remainItemCount, last));
			}
		}

		public static void Handle_ItemTransform(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			try
			{
				CItemTransformInfo itemTransformInfo = new CItemTransformInfo();
				if (ShopItemTable.getItemDataFromItemDescNum(num, out var itemData))
				{
					if (!itemData.isPosition(eFuncItemPosition.eFuncItemPosition_ITEM_TRANSFORM_ITEM))
					{
						return;
					}
					int num2 = (int)itemData.m_mapAttr[137];
					if (ShopItemTable.getItemQueryFromItemDescNum(num2, out var query))
					{
						query.Item3 = (int)itemData.m_mapAttr[136];
						if (!TransformItemManager.getItemTransformInfo(query, ref itemTransformInfo))
						{
							Client.SendAsync(new ItemTransformFail_ACK(3, last));
							return;
						}
					}
					if (transformItem(currentAccount, num, num2, itemTransformInfo.m_iTransKind, (short)itemTransformInfo.m_transformType))
					{
						Client.SendAsync(new ItemTransform_ACK(itemTransformInfo, num, last));
					}
					else
					{
						Client.SendAsync(new ItemTransformFail_ACK(2, last));
					}
				}
				else
				{
					Client.SendAsync(new ItemTransformFail_ACK(4, last));
				}
			}
			catch (Exception ex)
			{
				Log.Error("Handle_ItemTransform Error:{0} userNum:{1}, itemNum:{2}", ex.Message, currentAccount.UserNum, num);
				Client.SendAsync(new ItemTransformFail_ACK(2, last));
			}
		}

		public static void GroupItemStrengthenAttr(Account User)
		{
			User.UserItemStrengthenSlotAttrGroup.Clear();
			foreach (int key in User.UserItemStrengthenSlotAttr.Keys)
			{
				List<ItemAttr> value = (from g in User.UserItemStrengthenSlotAttr[key]
					group g by g.Attr into g
					select new ItemAttr
					{
						Attr = g.Key,
						AttrValue = g.Sum((ItemAttr s) => s.AttrValue)
					}).ToList();
				User.UserItemStrengthenSlotAttrGroup.TryAdd(key, value);
			}
		}

		public static void DBRequestItemStrengthenGetMyItem(Account User, int ItemNum)
		{
			bool flag = false;
			flag = itemStrengthen_getUserItem(User, ItemNum);
			if (flag)
			{
				flag = itemStrengthen_getUserItemSlot(User, ItemNum);
			}
			if (ItemNum != -1)
			{
				return;
			}
			if (flag)
			{
				User.Connection.SendAsync(new GET_USER_ITEM_ATTR_ACK(User, 1));
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, 1), room.RoomServerID);
				}
			}
			else
			{
				User.Connection.SendAsync(new GET_USER_ITEM_ATTR_ACK(User, eServerResult.eServerResult_GET_USER_ITEM_STRENGTHEN_FAILED_ACK, 1));
			}
		}

		public static bool itemStrengthen_getUserItem(Account User, int ItemNum)
		{
			bool flag = false;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemStrengthen_getUserItem";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["itemNum"]);
					ItemStrengthen info = new ItemStrengthen
					{
						strengthenCount = Convert.ToInt16(mySqlDataReader["strengthenCount"]),
						strengthenSlotCount = Convert.ToByte(mySqlDataReader["strengthenSlotCount"]),
						maxStrengthenSlotNum = Convert.ToByte(mySqlDataReader["maxStrengthenSlotNum"])
					};
					User.UserItemStrengthen.AddOrUpdate(key, info, delegate(int k, ItemStrengthen v)
					{
						v = info;
						return info;
					});
				}
				return true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_itemStrengthen_getUserItem Error: {0}", ex.Message);
				return flag;
			}
		}

		public static bool itemStrengthen_getUserItemSlot(Account User, int ItemNum)
		{
			bool flag = false;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemStrengthen_getUserItemSlot";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int num = Convert.ToInt32(mySqlDataReader["itemNum"]);
					byte b = Convert.ToByte(mySqlDataReader["slotNum"]);
					byte value = Convert.ToByte(mySqlDataReader["group"]);
					ItemAttr attr = new ItemAttr
					{
						slotNum = b,
						Attr = Convert.ToUInt16(mySqlDataReader["attrType"]),
						AttrValue = Convert.ToSingle(mySqlDataReader["attrValue"]),
						attrKind = Convert.ToByte(mySqlDataReader["attrKind"])
					};
					User.UserItemStrengthenSlotGroup[num][b] = value;
					User.UserItemStrengthenSlotAttr.AddOrUpdate(num, new List<ItemAttr> { attr }, delegate(int k, List<ItemAttr> v)
					{
						v.Add(attr);
						return v;
					});
				}
				return true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_itemStrengthen_getUserItemSlot Error: {0}", ex.Message);
				return flag;
			}
		}

		private static bool transformItem(Account User, int itemNum, int targetItemNum, int transKindNum, short transType)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_transformItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
					mySqlCommand.Parameters.Add("targetItemNum", MySqlDbType.Int32).Value = targetItemNum;
					mySqlCommand.Parameters.Add("transKindNum", MySqlDbType.Int32).Value = transKindNum;
					mySqlCommand.Parameters.Add("transType", MySqlDbType.Int16).Value = transType;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_transformItem error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ItemStrengthen_StrengthenSlot(Account User, int itemNum, short slotNum, bool failed, out short remainStrengthenCount)
		{
			remainStrengthenCount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_itemStrengthen_strengthenSlot";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("lucky", MySqlDbType.Int32).Value = (int)User.Luck;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
					mySqlCommand.Parameters.Add("slotNum", MySqlDbType.Int32).Value = slotNum;
					mySqlCommand.Parameters.Add("failed", MySqlDbType.Int16).Value = failed;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						long tR = User.TR;
						byte maxStrengthenSlotNum = 0;
						while (mySqlDataReader.Read())
						{
							tR = Convert.ToInt64(mySqlDataReader["gameMoney"]);
							byte b = Convert.ToByte(mySqlDataReader["slotNum"]);
							byte value = Convert.ToByte(mySqlDataReader["group"]);
							ItemAttr attr = new ItemAttr
							{
								slotNum = b,
								Attr = Convert.ToUInt16(mySqlDataReader["attrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["attrValue"]),
								attrKind = Convert.ToByte(mySqlDataReader["attrKind"])
							};
							remainStrengthenCount = Convert.ToInt16(mySqlDataReader["remainStrengthenCount"]);
							maxStrengthenSlotNum = Convert.ToByte(mySqlDataReader["maxMySlotNum"]);
							User.UserItemStrengthenSlotGroup[itemNum][b] = value;
							User.UserItemStrengthenSlotAttr.AddOrUpdate(itemNum, new List<ItemAttr> { attr }, delegate(int k, List<ItemAttr> v)
							{
								v.Add(attr);
								return v;
							});
						}
						User.TR = tR;
						User.UserItemStrengthen[itemNum].strengthenCount = remainStrengthenCount;
						User.UserItemStrengthen[itemNum].maxStrengthenSlotNum = maxStrengthenSlotNum;
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemStrengthen_strengthenSlot error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ItemStrengthen_CleanSlot(Account User, int itemNum, int cleanItemNum, int slotNum, short cleanType, out int maxMySlotNum)
		{
			maxMySlotNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_itemStrengthen_cleanSlot";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
					mySqlCommand.Parameters.Add("cleanItemNum", MySqlDbType.Int32).Value = cleanItemNum;
					mySqlCommand.Parameters.Add("slotNum", MySqlDbType.Int32).Value = slotNum;
					mySqlCommand.Parameters.Add("cleanType", MySqlDbType.Int16).Value = cleanType;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						long tR = User.TR;
						while (mySqlDataReader.Read())
						{
							tR = Convert.ToInt64(mySqlDataReader["gameMoney"]);
							maxMySlotNum = Convert.ToInt32(mySqlDataReader["maxMySlotNum"]);
						}
						User.TR = tR;
						User.UserItemStrengthen[itemNum].maxStrengthenSlotNum = (byte)maxMySlotNum;
						User.UserItemStrengthenSlotGroup.Remove(itemNum, (byte)slotNum);
						User.UserItemStrengthenSlotAttr[itemNum].RemoveAll((ItemAttr r) => r.slotNum == slotNum);
						if (slotNum >= 7)
						{
							User.UserItemStrengthenSlotGroup.Remove(itemNum, 0);
							User.UserItemStrengthenSlotAttr[itemNum].RemoveAll((ItemAttr r) => r.slotNum == 0);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemStrengthen_cleanSlot error: {0}", ex.Message);
				return false;
			}
		}

		private static bool IncreaseStrengthenCount(Account User, int itemNum, int increaseItemNum, out short strengthenCount, out int remainItemCount)
		{
			strengthenCount = 0;
			remainItemCount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_itemStrengthen_increaseStrengthenCount";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
					mySqlCommand.Parameters.Add("increaseItemNum", MySqlDbType.Int32).Value = increaseItemNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						remainItemCount = Convert.ToInt32(mySqlDataReader["remainItemCount"]);
						strengthenCount = Convert.ToInt16(mySqlDataReader["strengthenCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemStrengthen_increaseStrengthenCount error: {0}", ex.Message);
				itemStrengthen_getUserItem(User, itemNum);
				return false;
			}
		}
	}
}
