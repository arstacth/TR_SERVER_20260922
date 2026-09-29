using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using AgentServer.Packet;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Shop;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using Quartz;
using Serilog;

namespace AgentServer.Holders
{
	public static class CombinationShopHolder
	{
		private static readonly object elock = new object();

		private static ConcurrentDictionary<int, CombinationShopInfoEvent> EventScheduleInfo { get; set; } = new ConcurrentDictionary<int, CombinationShopInfoEvent>();


		public static ConcurrentDictionary<int, CombinationShopItemDetailInfo> ExchangeItemInfo { get; set; } = new ConcurrentDictionary<int, CombinationShopItemDetailInfo>();


		private static ConcurrentDictionary<int, int> m_systemUseable { get; set; } = new ConcurrentDictionary<int, int>();


		private static NestedDictionary<int, int, Tuple<int, int>> m_mExchangeCountList { get; set; } = new NestedDictionary<int, int, Tuple<int, int>>();


		public static void LoadCombinationShopInfo()
		{
			try
			{
				EventScheduleInfo.Clear();
				ExchangeItemInfo.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_CombinationShop_Info";
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						CombinationShopInfoEvent value = default(CombinationShopInfoEvent);
						value.m_iEventNum = mySqlDataReader.GetInt32("eventNum");
						value.m_schedule.m_iScheduleNum = mySqlDataReader.GetInt32("scheduleNum");
						value.m_schedule.m_StartDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("startDate"));
						value.m_schedule.m_EndDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("endDate"));
						if (value.m_schedule.m_StartDate < 946684800000L)
						{
							value.m_schedule.m_StartDate = 946684800000L;
						}
						if (value.m_schedule.m_EndDate < value.m_schedule.m_StartDate || value.m_schedule.m_EndDate > 2145916800000L)
						{
							value.m_schedule.m_EndDate = 2145916800000L;
						}
						value.m_SystemType = mySqlDataReader.GetInt32("systemNum");
						value.m_iStartHour = mySqlDataReader.GetByte("startHour");
						value.m_iStartMinute = mySqlDataReader.GetByte("startMinute");
						value.m_iEndHour = mySqlDataReader.GetByte("endHour");
						value.m_iEndMinute = mySqlDataReader.GetByte("endMinute");
						bool flag = (value.m_bIsNotify = mySqlDataReader.GetBoolean("isNotify"));
						value.m_systemName = mySqlDataReader.GetString("systemName");
						if (EventScheduleInfo.ContainsKey(value.m_SystemType))
						{
							EventScheduleInfo[value.m_SystemType] = value;
						}
						else
						{
							EventScheduleInfo.TryAdd(value.m_SystemType, value);
						}
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						CombinationShopItemDetailInfo value2 = default(CombinationShopItemDetailInfo);
						value2.m_iExchangeID = mySqlDataReader.GetInt32("exchangeID");
						value2.m_iExchangeItem = mySqlDataReader.GetInt32("exchangeItem");
						value2.m_StartDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("startDate"));
						value2.m_EndDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("endDate"));
						value2.m_iLimitSellCount = mySqlDataReader.GetInt32("limitSellCount");
						value2.m_iLimitUserBuyCount = mySqlDataReader.GetInt32("limitUserBuyCount");
						// Keep wire dates in a sane unix-ms window (overflow → client NOT_USE_TIME).
						if (value2.m_StartDate < 946684800000L)
						{
							value2.m_StartDate = 946684800000L;
						}
						if (value2.m_EndDate < value2.m_StartDate || value2.m_EndDate > 2145916800000L)
						{
							value2.m_EndDate = 2145916800000L;
						}
						ExchangeItemInfo[value2.m_iExchangeID] = value2;
					}
				}
				Log.Information("Load Combination Shop Info OK!!");
			}
			catch (Exception ex)
			{
				Log.Error("Load CombinationShop Info Error : {0}", ex.ToString());
			}
			// Enable systems immediately — Quartz is every 10s and ActorOf can fail on restart rename.
			scheduleCheck();
			try
			{
				IActorRef to = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new CombinationShopManager()), "scheduleCheck");
				ServerStatus.QuartzActor.Tell(new CreateJob(to, "", TriggerBuilder.Create().WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
				{
					x.WithIntervalInSeconds(10).RepeatForever();
				}).Build()));
			}
			catch (Exception ex)
			{
				Log.Error("CombinationShop schedule actor: {0}", ex.Message);
			}
		}

		public static eCombinationShopResult getUseInfo(int systemType)
		{
			if (m_systemUseable.ContainsKey(systemType))
			{
				return eCombinationShopResult.eCombinationShopResult_OK;
			}
			// Any scheduled system is usable — date math overflow was returning CAN_NOT_USE
			// and the client shows that as error msg 5 (NOT_USE_TIME).
			if (EventScheduleInfo.TryGetValue(systemType, out CombinationShopInfoEvent ev))
			{
				m_systemUseable[systemType] = ev.m_iEventNum;
				return eCombinationShopResult.eCombinationShopResult_OK;
			}
			return eCombinationShopResult.eCombinationShopResult_CAN_NOT_USE;
		}

		public static int getEventNum(int systemType)
		{
			if (m_systemUseable.ContainsKey(systemType))
			{
				return m_systemUseable[systemType];
			}
			return 0;
		}

		public static bool isExchangeCountCheck(int systemType, int exchangeID)
		{
			if (!m_mExchangeCountList.ContainsKey(systemType))
			{
				return true;
			}
			if (m_mExchangeCountList[systemType].TryGetValue(exchangeID, out var value))
			{
				if (value.Item1 >= value.Item2 + 1)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public static void setExchangeCount(int systemType, int exchangeID, int iMaxSellCount, int iNowSellCount)
		{
			lock (elock)
			{
				if (!m_mExchangeCountList.ContainsKey(systemType))
				{
					m_mExchangeCountList.Add(systemType, exchangeID, new Tuple<int, int>(iMaxSellCount, iNowSellCount));
				}
				else if (m_mExchangeCountList[systemType].ContainsKey(exchangeID))
				{
					m_mExchangeCountList[systemType][exchangeID] = new Tuple<int, int>(iMaxSellCount, iNowSellCount);
				}
				else
				{
					m_mExchangeCountList[systemType].Add(exchangeID, new Tuple<int, int>(iMaxSellCount, iNowSellCount));
				}
			}
		}

		private static void resetExchangeCount(int systemType)
		{
			lock (elock)
			{
				if (m_mExchangeCountList.ContainsKey(systemType))
				{
					m_mExchangeCountList.Remove(systemType);
				}
			}
		}

		public static void scheduleCheck()
		{
			if (EventScheduleInfo.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<int, CombinationShopInfoEvent> item in EventScheduleInfo)
			{
				int key = item.Key;
				bool flag = false;
				bool flag2 = false;
				int num = 0;
				_ = string.Empty;
				long num2 = Utility.CurrentTimeMilliseconds();
				if (item.Value.m_schedule.m_StartDate <= num2 && item.Value.m_schedule.m_EndDate >= num2 && (DateTime.Now.Hour > item.Value.m_iStartHour || (DateTime.Now.Hour == item.Value.m_iStartHour && DateTime.Now.Minute >= item.Value.m_iStartMinute)) && (DateTime.Now.Hour < item.Value.m_iEndHour || (DateTime.Now.Hour == item.Value.m_iEndHour && DateTime.Now.Minute <= item.Value.m_iEndMinute)))
				{
					flag = true;
					num = item.Value.m_iEventNum;
					flag2 = item.Value.m_bIsNotify;
					_ = item.Value;
				}
				bool flag3 = m_systemUseable.ContainsKey(key);
				if (!flag && flag3)
				{
					Log.Debug("CombinationShop END {0} EventNum {1} ", key, num);
					if (flag2)
					{
						notifyAllClient(key, flag);
					}
					m_systemUseable.TryRemove(key, out var _);
					resetExchangeCount(key);
				}
				if (flag && !flag3)
				{
					Log.Debug("CombinationShop START {0} EventNum {1} ", key, num);
					m_systemUseable[key] = num;
					loadLimitCountInfo(key, num);
					if (flag2)
					{
						notifyAllClient(key, flag);
					}
				}
			}
		}

		private static void notifyAllClient(int combinationShop, bool bIsVisible)
		{
			ServerStatus.LBServerActor.Tell(new COMBINATION_SHOP_NOTIFY((byte)combinationShop, bIsVisible));
		}

		private static void loadLimitCountInfo(int systemNum, int eventNum)
		{
			NestedDictionary<int, int, Tuple<int, int>> nestedDictionary = new NestedDictionary<int, int, Tuple<int, int>>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_CombinationShop_CountInfo";
				mySqlCommand.Parameters.Add("systemNum", MySqlDbType.Int32).Value = systemNum;
				mySqlCommand.Parameters.Add("eventNum", MySqlDbType.Int32).Value = eventNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("eventNum");
					if (eventNum == 0 || eventNum == @int)
					{
						mySqlDataReader.GetInt32("systemNum");
						int int2 = mySqlDataReader.GetInt32("exchangeID");
						int int3 = mySqlDataReader.GetInt32("maxSellCount");
						int int4 = mySqlDataReader.GetInt32("nowSellCount");
						if (nestedDictionary.ContainsKey(systemNum))
						{
							nestedDictionary[systemNum][int2] = new Tuple<int, int>(int3, int4);
						}
						else
						{
							nestedDictionary.Add(systemNum, int2, new Tuple<int, int>(int3, int4));
						}
					}
				}
				initSellCount(systemNum, nestedDictionary);
			}
			catch (MySqlException ex)
			{
				Log.Error("usp_CombinationShop_CountInfo Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_CombinationShop_CountInfo Error:{0}", ex2.ToString());
			}
		}

		private static void initSellCount(int combinationShop, NestedDictionary<int, int, Tuple<int, int>> sellCount)
		{
			if (sellCount.TryGetValue(combinationShop, out var value))
			{
				lock (elock)
				{
					m_mExchangeCountList[combinationShop] = value;
					return;
				}
			}
			resetExchangeCount(combinationShop);
		}
	}
}
