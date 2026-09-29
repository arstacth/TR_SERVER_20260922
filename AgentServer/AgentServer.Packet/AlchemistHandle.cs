using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;
using Weighted_Randomizer;

namespace AgentServer.Packet
{
	public class AlchemistHandle
	{
		public static void Handle_AlchemistHistory(ClientConnection Client, PacketReader reader, byte last)
		{
			Alchemist_UserInfo(Client.CurrentAccount.UserNum, out var itemInfos);
			Client.SendAsync(new AlchemistHistory_ACK(itemInfos, last));
		}

		public static void Handle_AlchemistMix(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int resultItemDescNum = reader.ReadLEInt32();
			short alchemistMixGrade = GetAlchemistMixGrade((float)currentAccount.Luck);
			int luck = (int)Math.Round(currentAccount.Luck, MidpointRounding.AwayFromZero);
			if (AlchemistMix(currentAccount, resultItemDescNum, luck, alchemistMixGrade, out var m_itemAttrInfo, out var dwmCount, out var m_failReason))
			{
				Client.SendAsync(new AlchemistMix_ACK(m_itemAttrInfo, dwmCount, last));
				UpdateMiscellaneous(currentAccount.UserNum, (alchemistMixGrade == 100) ? 1 : 0, 1);
			}
			else
			{
				Client.SendAsync(new AlchemistMix_Fail(m_failReason, last));
			}
		}

		public static void Handle_AlchemistEnchantGrade(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (ItemHolder.AlchemistEnchantData.TryGetValue(num, out var value))
			{
				short alchemistEnchantGrade = GetAlchemistEnchantGrade((float)currentAccount.Luck, value);
				int luck = (int)Math.Round(currentAccount.Luck, MidpointRounding.AwayFromZero);
				if (AlchemistEnchantGrade(currentAccount, num, luck, alchemistEnchantGrade, out var m_itemAttrInfo, out var bSuccess, out var enchantToEpic, out var m_failReason))
				{
					Client.SendAsync(new AlchemistEnchantGrade_ACK(m_itemAttrInfo, bSuccess, last));
					UpdateMiscellaneous(currentAccount.UserNum, enchantToEpic ? 1 : 0, 1);
				}
				else
				{
					Client.SendAsync(new AlchemistEnchantGrade_Fail(m_failReason, last));
				}
			}
			else
			{
				Client.SendAsync(new AlchemistEnchantGrade_Fail(eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_HAVE_NO_ITEM, last));
			}
		}

		public static void Handle_AlchemistDisjoint(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (AlchemistDisjoint(currentAccount, num, out var exinfo))
			{
				Client.SendAsync(new AlchemistDisjoint_ACK(num, exinfo, last));
			}
			else
			{
				Client.SendAsync(new AlchemistDisjoint_Fail(last));
			}
		}

		public static void Handle_AlchemistQuickJoin(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int mapNum = reader.ReadLEInt32();
			IEnumerable<NormalRoom> source = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.MapNum == mapNum && rm.PlayerCount < rm.SlotCount && !rm.isPlaying && !rm.HasPassword && !rm.hasAfreecaTV);
			if (source.Count() == 0)
			{
				Client.SendAsync(new GameRoom_EnterRoomError(13, 0, last, 30));
				return;
			}
			NormalRoom normalRoom = source.OrderBy((NormalRoom _) => Guid.NewGuid()).FirstOrDefault();
			ServerStatus.ToRoomServer(new RM_PlayerEnterRoom(currentAccount, string.Empty, normalRoom.ID, normalRoom.RoomKindID, 0, 0, last), normalRoom.RoomServerID);
		}

		public static void GetMyAlchemistCards(int UserNum, string requestCardItemNums, out Dictionary<int, int> cards)
		{
			cards = new Dictionary<int, int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_myRoomGetMyCards";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("requestCardItemNums", MySqlDbType.VarString).Value = requestCardItemNums;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["cardnum"]);
					int value = Convert.ToInt32(mySqlDataReader["cardcount"]);
					cards.Add(key, value);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_myRoomGetMyCards Error:{0}", ex.ToString());
			}
		}

		private static short GetAlchemistMixGrade(float luck)
		{
			int num = 0;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			if (luck <= 0f)
			{
				num = random.Next() % 10001;
			}
			else
			{
				int num2 = 150;
				int num3 = (int)(luck / (float)num2 + 1f);
				num3 = ((num3 > 10) ? 10 : num3);
				for (int i = 0; i < num3; i++)
				{
					int num4 = random.Next() % 10001;
					if (num < num4)
					{
						num = num4;
					}
				}
			}
			int num5 = 9900;
			int num6 = 9500;
			int num7 = 8500;
			int num8 = 6000;
			if (num >= num5)
			{
				return 100;
			}
			if (num >= num6)
			{
				return 200;
			}
			if (num >= num7)
			{
				return 300;
			}
			if (num >= num8)
			{
				return 400;
			}
			return 500;
		}

		private static short GetAlchemistEnchantGrade(float luck, IWeightedRandomizer<short> item)
		{
			if (luck <= 0f)
			{
				return item.NextWithReplacement();
			}
			int num = 150;
			int num2 = (int)(luck / (float)num + 1f);
			num2 = ((num2 > 10) ? 10 : num2);
			short num3 = -1;
			for (int i = 0; i < num2; i++)
			{
				short num4 = item.NextWithReplacement();
				if (num3 < 0 || num3 > num4)
				{
					num3 = num4;
				}
			}
			return num3;
		}

		private static void Alchemist_UserInfo(int UserNum, out List<Tuple<int, short, short, short, short>> itemInfos)
		{
			itemInfos = new List<Tuple<int, short, short, short, short>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_alchemist_GetUserInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("ItemNum");
						short item = Convert.ToInt16(mySqlDataReader["DailyCnt"]);
						short item2 = Convert.ToInt16(mySqlDataReader["WeeklyCnt"]);
						short item3 = Convert.ToInt16(mySqlDataReader["MonthlyCnt"]);
						short item4 = Convert.ToInt16(mySqlDataReader["AccCnt"]);
						itemInfos.Add(new Tuple<int, short, short, short, short>(@int, item, item2, item3, item4));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_alchemist_GetUserInfoError:{0}", ex.ToString());
			}
		}

		private static bool AlchemistMix(Account User, int resultItemDescNum, int luck, short itemClass, out UserItemAttrInfo m_itemAttrInfo, out Tuple<short, short, short> dwmCount, out eAlchemistMixFailedReason m_failReason)
		{
			dwmCount = null;
			m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_UNKNOWN;
			m_itemAttrInfo = new UserItemAttrInfo();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_alchemist_mix_new");
				mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("resultItemDescNum", resultItemDescNum);
				mySqlCommandHelper.AddParamInt("luck", luck);
				mySqlCommandHelper.AddParamShort("itemClass", itemClass);
				mySqlCommandHelper.AddParamBoolean("pforce", value: false);
				mySqlCommandHelper.AddParamBoolean("noResultSet", value: false);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					if (@short != 0)
					{
						m_itemAttrInfo.m_ItemAttr[@short] = @float;
					}
				}
				mySqlCommandHelper.NextResult();
				if (mySqlCommandHelper.HasResult())
				{
					m_itemAttrInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("ResultItemDescNum");
					short shortConvert = mySqlCommandHelper.GetShortConvert("DailyCount");
					short shortConvert2 = mySqlCommandHelper.GetShortConvert("WeeklyCount");
					short shortConvert3 = mySqlCommandHelper.GetShortConvert("MonthlyCount");
					dwmCount = new Tuple<short, short, short>(shortConvert, shortConvert2, shortConvert3);
				}
				flag = true;
			}
			catch (MySqlException ex)
			{
				flag = false;
				if (ex.Message.Contains("Invalid Alchemist Item"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_CANNOT_FIND_RESULTITEM;
				}
				else if (ex.Message.Contains("Limit Over"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_OVER_MAKING_LIMIT;
				}
				else if (ex.Message.Contains("No UnconsumedItem"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_NOT_ENOUGH_UNCONSUMED_ITEM;
				}
				else if (ex.Message.Contains("No Mix Condition") || ex.Message.Contains("No Round Position"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_NO_MIX_CONDITION;
				}
				else if (ex.Message.Contains("Duplicate"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_DUPLICATE_RESULTITEM;
				}
				else if (ex.Message.Contains("Not Enough Source"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_NO_NEED_ITEMS;
				}
				else if (ex.Message.Contains("Item Give Error"))
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_CANNOT_GIVE_RESULT_ITEM;
				}
				else
				{
					m_failReason = eAlchemistMixFailedReason.eAlchemistMixFailedReason_UNKNOWN;
				}
				Log.Error("usp_alchemist_mix_new Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				flag = false;
				Log.Error("usp_alchemist_mix_new Error:{0}", ex2.ToString());
			}
			if (flag && m_itemAttrInfo.m_ItemAttr.Count > 0)
			{
				User.userItemAttr.insertItemAttr(m_itemAttrInfo);
				User.mixUserItemAttr();
			}
			return flag;
		}

		private static bool AlchemistEnchantGrade(Account User, int ItemNum, int luck, short itemClass, out UserItemAttrInfo m_itemAttrInfo, out bool bSuccess, out bool enchantToEpic, out eAlchemistEnchantGradeFailedReason m_failReason)
		{
			bSuccess = false;
			enchantToEpic = false;
			m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_UNKNOWN;
			m_itemAttrInfo = new UserItemAttrInfo();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_alchemist_enchantGrade_new");
				mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("enchantReqItemNum", ItemNum);
				mySqlCommandHelper.AddParamInt("lucky", luck);
				mySqlCommandHelper.AddParamInt("itemClass", itemClass);
				mySqlCommandHelper.Execute();
				m_itemAttrInfo.m_iItemDescNum = ItemNum;
				while (mySqlCommandHelper.HasResult())
				{
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					if (@short != 0)
					{
						m_itemAttrInfo.m_ItemAttr[@short] = @float;
					}
				}
				mySqlCommandHelper.NextResult();
				if (mySqlCommandHelper.HasResult())
				{
					bSuccess = mySqlCommandHelper.GetBoolean("Success");
					enchantToEpic = mySqlCommandHelper.GetBoolean("EnchantToEpic");
				}
				flag = true;
			}
			catch (MySqlException ex)
			{
				flag = false;
				if (ex.Message.Contains("No Item"))
				{
					m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_HAVE_NO_ITEM;
				}
				else if (ex.Message.Contains("No Alchemist Item"))
				{
					m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_NO_ALCHEMIST_ITEM;
				}
				else if (ex.Message.Contains("No Mix Condition") || ex.Message.Contains("No Round Position"))
				{
					m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_NO_MIX_CONDITION;
				}
				else if (ex.Message.Contains("Not Enough Source"))
				{
					m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_NO_NEED_ITEMS;
				}
				else
				{
					m_failReason = eAlchemistEnchantGradeFailedReason.eAlchemistEnchantGradeFailedReason_UNKNOWN;
				}
				Log.Error("usp_alchemist_enchantGrade_new Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				flag = false;
				Log.Error("usp_alchemist_enchantGrade_new Error:{0}", ex2.ToString());
			}
			if (flag && m_itemAttrInfo.m_ItemAttr.Count > 0)
			{
				User.userItemAttr.insertItemAttr(m_itemAttrInfo);
				User.mixUserItemAttr();
			}
			return flag;
		}

		private static bool AlchemistDisjoint(Account User, int ItemNum, out List<ExchangeItemInfo> exinfo)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_alchemist_disjoint_new";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemDescNum", MySqlDbType.Int32).Value = ItemNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						ExchangeItemInfo item = new ExchangeItemInfo
						{
							type = Convert.ToInt32(mySqlDataReader["rewardType"]),
							id = Convert.ToInt32(mySqlDataReader["rewardItem"]),
							count = Convert.ToInt32(mySqlDataReader["rewardCount"])
						};
						exinfo.Add(item);
					}
				}
				return true;
			}
			catch (MySqlException ex)
			{
				Log.Error("usp_alchemist_disjoint_new Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_alchemist_disjoint_new Error:{0}", ex2.ToString());
			}
			return false;
		}

		private static bool UpdateMiscellaneous(int Usernum, int sclassCount = 0, int alchemistCount = 0, int cleanCount = 0, int enchantCount = 0, int growthPotionCount = 0, int harvestCount = 0, int licenseChallengeCount = 0, int nutritionPotionCount = 0)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_IndividualRecordUpdateMiscellaneous";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = Usernum;
					mySqlCommand.Parameters.Add("sclassCount", MySqlDbType.Int16).Value = sclassCount;
					mySqlCommand.Parameters.Add("alchemistCount", MySqlDbType.Int32).Value = alchemistCount;
					mySqlCommand.Parameters.Add("cleanCount", MySqlDbType.Int32).Value = cleanCount;
					mySqlCommand.Parameters.Add("enchantCount", MySqlDbType.Int32).Value = enchantCount;
					mySqlCommand.Parameters.Add("growthPotionCount", MySqlDbType.Int32).Value = growthPotionCount;
					mySqlCommand.Parameters.Add("harvestCount", MySqlDbType.Int32).Value = harvestCount;
					mySqlCommand.Parameters.Add("licenseChallengeCount", MySqlDbType.Int32).Value = licenseChallengeCount;
					mySqlCommand.Parameters.Add("nutritionPotionCount", MySqlDbType.Int32).Value = nutritionPotionCount;
					mySqlCommand.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Error on usp_IndividualRecordUpdateMiscellaneous: {0}", ex.Message);
				return false;
			}
		}
	}
}
