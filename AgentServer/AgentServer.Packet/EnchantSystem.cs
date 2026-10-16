using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Database;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class EnchantSystem
	{
		public static void Handle_GetEnchantItemInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.ReadLEInt32();
			Enchant_GetItemInfo(currentAccount, itemNum, last);
		}

		public static void Handle_Hardening(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int stoneNum = reader.ReadLEInt32();
			byte type = reader.ReadByte();
			if (Enchant_Hardening(currentAccount, stoneNum, type, out var ResultStoneNum))
			{
				Client.SendAsync(new HardeningDone(currentAccount.TR, stoneNum, ResultStoneNum, last));
			}
		}

		public static void Handle_StoneMount(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.ReadLEInt32();
			byte pSeqNum = reader.ReadByte();
			int pStoneNum = reader.ReadLEInt32();
			Enchant_StoneMount(currentAccount, itemNum, pStoneNum, pSeqNum, last);
		}

		public static void Handle_StoneRemove(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int itemNum = reader.ReadLEInt32();
			byte pSeqNum = reader.ReadByte();
			Enchant_StoneRemove(currentAccount, itemNum, pSeqNum, last);
		}

		public static void Handle_SealErase(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int sealEraseItem = reader.ReadLEInt32();
			int itemNum = reader.ReadLEInt32();
			byte pSeqNum = reader.ReadByte();
			Enchant_SealErase(currentAccount, sealEraseItem, itemNum, pSeqNum, last);
		}

		private static bool Enchant_GetItemInfo(int UserNum, out NestedDictionary<int, byte, byte, int, List<ItemAttr>> infos, out Dictionary<int, List<ItemAttr>> attrs)
		{
			infos = new NestedDictionary<int, byte, byte, int, List<ItemAttr>>();
			attrs = new Dictionary<int, List<ItemAttr>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_enchant_getUserInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						int num = Convert.ToInt32(mySqlDataReader["fdItemDescNum"]);
						byte b = Convert.ToByte(mySqlDataReader["fdSeqNum"]);
						byte b2 = Convert.ToByte(mySqlDataReader["fdSocketNum"]);
						int num2 = Convert.ToInt32(mySqlDataReader["fdStoneNum"]);
						ItemAttr item = new ItemAttr
						{
							Attr = Convert.ToUInt16(mySqlDataReader["fdAttrType"]),
							AttrValue = Convert.ToSingle(mySqlDataReader["fdAttrValue"])
						};
						if (!infos.ContainsKey(num, b, b2, num2))
						{
							infos.Add(num, b, b2, num2, new List<ItemAttr> { item });
						}
						else
						{
							infos[num][b][b2][num2].Add(item);
						}
						if (!attrs.ContainsKey(num))
						{
							attrs.Add(num, new List<ItemAttr> { item });
						}
						else
						{
							attrs[num].Add(item);
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_enchant_getUserInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool Enchant_Hardening(Account User, int StoneNum, int type, out int ResultStoneNum)
		{
			ResultStoneNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_enchant_hardening";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("StoneNum", MySqlDbType.Int32).Value = StoneNum;
					mySqlCommand.Parameters.Add("type", MySqlDbType.Int32).Value = type;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						int @int = mySqlDataReader.GetInt32("CostType");
						int int2 = mySqlDataReader.GetInt32("Cost");
						ResultStoneNum = mySqlDataReader.GetInt32("ReturnRealStoneNum");
						if (@int == 0)
						{
							User.TR -= int2;
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_enchant_hardening Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool Enchant_StoneMount(Account User, int ItemNum, int pStoneNum, short pSeqNum, out NestedDictionary<byte, byte, int, List<ItemAttr>> infos, out List<ItemAttr> Attrs)
		{
			infos = new NestedDictionary<byte, byte, int, List<ItemAttr>>();
			Attrs = new List<ItemAttr>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_enchant_mount";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("StoneNum", MySqlDbType.Int32).Value = pStoneNum;
					mySqlCommand.Parameters.Add("SeqNum", MySqlDbType.Int16).Value = pSeqNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							byte key = Convert.ToByte(mySqlDataReader["fdSeqNum"]);
							byte b = Convert.ToByte(mySqlDataReader["fdSocketNum"]);
							int num = Convert.ToInt32(mySqlDataReader["fdStoneNum"]);
							ItemAttr item = new ItemAttr
							{
								Attr = Convert.ToUInt16(mySqlDataReader["fdAttrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["fdAttrValue"])
							};
							if (!infos.ContainsKey(key, b, num))
							{
								infos.Add(key, b, num, new List<ItemAttr> { item });
							}
							else
							{
								infos[key][b][num].Add(item);
							}
						}
						mySqlDataReader.NextResult();
						while (mySqlDataReader.Read())
						{
							ItemAttr item2 = new ItemAttr
							{
								Attr = Convert.ToUInt16(mySqlDataReader["AttrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["AttrValue"])
							};
							Attrs.Add(item2);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_enchant_mount Error:{0}, StoneNum:{1}", ex.Message, pStoneNum);
				return false;
			}
		}

		private static bool Enchant_StoneRemove(Account User, int ItemNum, short pSeqNum, out int ReturnStoneNum, out NestedDictionary<byte, byte, int, List<ItemAttr>> infos, out List<ItemAttr> Attrs)
		{
			infos = new NestedDictionary<byte, byte, int, List<ItemAttr>>();
			Attrs = new List<ItemAttr>();
			ReturnStoneNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_enchant_remove";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("SeqNum", MySqlDbType.Int16).Value = pSeqNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							byte key = Convert.ToByte(mySqlDataReader["fdSeqNum"]);
							byte b = Convert.ToByte(mySqlDataReader["fdSocketNum"]);
							int num = Convert.ToInt32(mySqlDataReader["fdStoneNum"]);
							ItemAttr item = new ItemAttr
							{
								Attr = Convert.ToUInt16(mySqlDataReader["fdAttrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["fdAttrValue"])
							};
							ReturnStoneNum = Convert.ToInt32(mySqlDataReader["StoneItem"]);
							if (!infos.ContainsKey(key, b, num))
							{
								infos.Add(key, b, num, new List<ItemAttr> { item });
							}
							else
							{
								infos[key][b][num].Add(item);
							}
						}
						mySqlDataReader.NextResult();
						while (mySqlDataReader.Read())
						{
							ItemAttr item2 = new ItemAttr
							{
								Attr = Convert.ToUInt16(mySqlDataReader["AttrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["AttrValue"])
							};
							Attrs.Add(item2);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_enchant_remove Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool Enchant_SealErase(Account User, int SealEraseItem, int ItemNum, short pSeqNum, out NestedDictionary<byte, byte, int, List<ItemAttr>> infos)
		{
			infos = new NestedDictionary<byte, byte, int, List<ItemAttr>>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_enchant_SealErase";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("SealEraseItem", MySqlDbType.Int32).Value = SealEraseItem;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("SeqNum", MySqlDbType.Int16).Value = pSeqNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						int num = 0;
						int num2 = 0;
						while (mySqlDataReader.Read())
						{
							byte key = Convert.ToByte(mySqlDataReader["fdSeqNum"]);
							byte b = Convert.ToByte(mySqlDataReader["fdSocketNum"]);
							int num3 = Convert.ToInt32(mySqlDataReader["fdStoneNum"]);
							ItemAttr item = new ItemAttr
							{
								Attr = Convert.ToUInt16(mySqlDataReader["fdAttrType"]),
								AttrValue = Convert.ToSingle(mySqlDataReader["fdAttrValue"])
							};
							if (!infos.ContainsKey(key, b, num3))
							{
								infos.Add(key, b, num3, new List<ItemAttr> { item });
							}
							else
							{
								infos[key][b][num3].Add(item);
							}
							num = mySqlDataReader.GetInt32("CostType");
							num2 = mySqlDataReader.GetInt32("Cost");
						}
						if (num == 0)
						{
							User.TR -= num2;
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_enchant_SealErase Error:{0}", ex.Message);
				return false;
			}
		}

		private static void Enchant_GetItemInfo(Account User, int ItemNum, byte last = 1)
		{
			int num = 0;
			int num2 = 0;
			byte b = 0;
			byte b2 = 0;
			short num3 = 0;
			float num4 = 0f;
			Dictionary<int, UserEnchantItem> dictionary = new Dictionary<int, UserEnchantItem>();
			eENCHANT_SYSTEM_RESULT eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_enchant_getUserInfo");
				mySqlCommandHelper.AddParamInt("UserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("ItemNum", ItemNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					num = mySqlCommandHelper.GetInt("fdItemDescNum");
					b = mySqlCommandHelper.GetByte("fdSeqNum");
					b2 = mySqlCommandHelper.GetByte("fdSocketNum");
					num2 = mySqlCommandHelper.GetInt("fdStoneNum");
					num3 = mySqlCommandHelper.GetShort("fdAttrType");
					num4 = mySqlCommandHelper.GetFloat("fdAttrValue");
					if (dictionary.ContainsKey(num))
					{
						dictionary[num].append(b, b2, num2, num3, num4);
						continue;
					}
					UserEnchantItem userEnchantItem = new UserEnchantItem();
					userEnchantItem.set(num);
					userEnchantItem.append(b, b2, num2, num3, num4);
					dictionary.Add(num, userEnchantItem);
				}
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK;
			}
			catch (Exception ex)
			{
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
				Log.Error("usp_enchant_getUserInfo Error:{0}", ex.Message);
			}
			if (eENCHANT_SYSTEM_RESULT == eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK)
			{
				User.setEnchantItem(ItemNum, dictionary);
				User.Connection.SendAsync(new EnchantItemInfo(ItemNum, dictionary, last));
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, last), room.RoomServerID);
				}
			}
		}

		private static void Enchant_StoneMount(Account User, int ItemNum, int pStoneNum, short pSeqNum, byte last = 1)
		{
			eENCHANT_SYSTEM_RESULT eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
			byte b = 0;
			byte iSocketNum = 0;
			int iMountStoneNum = 0;
			Dictionary<short, float> dictionary = new Dictionary<short, float>();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_enchant_mount");
				mySqlCommandHelper.AddParamInt("UserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("ItemNum", ItemNum);
				mySqlCommandHelper.AddParamInt("StoneNum", pStoneNum);
				mySqlCommandHelper.AddParamShort("SeqNum", pSeqNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					b = mySqlCommandHelper.GetByte("fdSeqNum");
					iSocketNum = mySqlCommandHelper.GetByte("fdSocketNum");
					iMountStoneNum = mySqlCommandHelper.GetInt("fdStoneNum");
					short @short = mySqlCommandHelper.GetShort("fdAttrType");
					float @float = mySqlCommandHelper.GetFloat("fdAttrValue");
					if (!dictionary.ContainsKey(@short))
					{
						dictionary.Add(@short, @float);
					}
				}
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK;
			}
			catch (Exception ex)
			{
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
				Log.Error("usp_enchant_mount Error:{0}, StoneNum:{1}", ex.Message, pStoneNum);
			}
			if (eENCHANT_SYSTEM_RESULT != eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK)
			{
				User.Connection.SendAsync(new StoneMountFail(eENCHANT_SYSTEM_RESULT, last));
				return;
			}
			User.mountEnchantItem(ItemNum, b, iSocketNum, iMountStoneNum, dictionary, out var enchantItem);
			User.Connection.SendAsync(new StoneMountSuccess(User.TR, b, enchantItem, last));
			if (User.isInRoom(out var room))
			{
				ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, last), room.RoomServerID);
			}
		}

		private static void Enchant_StoneRemove(Account User, int ItemNum, short pSeqNum, byte last = 1)
		{
			eENCHANT_SYSTEM_RESULT eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
			int returnStoneNum = 0;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_enchant_remove");
				mySqlCommandHelper.AddParamInt("UserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("ItemNum", ItemNum);
				mySqlCommandHelper.AddParamShort("SeqNum", pSeqNum);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					returnStoneNum = mySqlCommandHelper.GetInt("StoneItem");
				}
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK;
			}
			catch (Exception ex)
			{
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
				Log.Error("usp_enchant_remove Error:{0}", ex.Message);
			}
			if (eENCHANT_SYSTEM_RESULT == eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK)
			{
				User.removeEnchantItemStone(ItemNum, (byte)pSeqNum, out var enchantItem);
				User.Connection.SendAsync(new StoneRemoveSuccess(returnStoneNum, enchantItem, last));
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, last), room.RoomServerID);
				}
			}
		}

		private static void Enchant_SealErase(Account User, int SealEraseItem, int ItemNum, short pSeqNum, byte last = 1)
		{
			eENCHANT_SYSTEM_RESULT eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
			byte iSocketNum = 0;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_enchant_SealErase");
				mySqlCommandHelper.AddParamInt("UserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("SealEraseItem", SealEraseItem);
				mySqlCommandHelper.AddParamInt("ItemNum", ItemNum);
				mySqlCommandHelper.AddParamShort("SeqNum", pSeqNum);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					iSocketNum = mySqlCommandHelper.GetByte("UnSealedSocketNum");
				}
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK;
			}
			catch (Exception ex)
			{
				eENCHANT_SYSTEM_RESULT = eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_DBERROR;
				Log.Error("usp_enchant_SealErase Error:{0}", ex.Message);
			}
			if (eENCHANT_SYSTEM_RESULT == eENCHANT_SYSTEM_RESULT.eENCHANT_SYSTEM_RESULT_OK)
			{
				User.TRNeedUpdateFromDB = true;
				User.mountEnchantItem(ItemNum, (byte)pSeqNum, iSocketNum, 0, new Dictionary<short, float>(), out var enchantItem);
				User.Connection.SendAsync(new SealEraseISuccess(SealEraseItem, enchantItem, last));
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, last), room.RoomServerID);
				}
			}
		}
	}
}
