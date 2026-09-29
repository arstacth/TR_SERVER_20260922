using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Quartz;
using Serilog;

namespace AgentServer.Holders
{
	public static class ShopHolder
	{
		private static IActorRef ShopActor;

		public static List<ShopCategory> ShopCategoryList { get; set; } = new List<ShopCategory>();


		public static List<ShopCategoryPurchasing> ShopCategoryPurchasingList { get; set; } = new List<ShopCategoryPurchasing>();


		public static List<ShopDisplayItem> ShopDisplayItemList { get; set; } = new List<ShopDisplayItem>();


		public static Dictionary<int, ShopDisplayItem> ShopDisplayItemListDict { get; set; } = new Dictionary<int, ShopDisplayItem>();


		public static Dictionary<int, ShopItemSell> ShopItemSellList { get; set; } = new Dictionary<int, ShopItemSell>();


		public static Dictionary<int, ShopDisplayDateLimit> ShopDisplayDateLimitList { get; set; } = new Dictionary<int, ShopDisplayDateLimit>();


		public static List<ShopBuyLimitCount> ShopBuyLimitCountList { get; set; } = new List<ShopBuyLimitCount>();


		public static List<ShopBuyAddBenefit> ShopBuyAddBenefitList { get; set; } = new List<ShopBuyAddBenefit>();


		public static HashSet<int> NotForSaleList { get; set; } = new HashSet<int>();


		public static List<GameDataShopPurchasingLimit> ShopPurchasingLimitList { get; set; } = new List<GameDataShopPurchasingLimit>();


		public static List<ShopCategoryLimit> ShopCategoryLimits { get; set; } = new List<ShopCategoryLimit>();


		public static Dictionary<int, List<int>> SelectivePackageList { get; set; } = new Dictionary<int, List<int>>();


		// Client crashes on huge 1608 (~156KB). Keep ~40–50KB while filling every Category1 tab.
		private const int PackedShopMaxTotal = 900;

		private const int PackedShopMinPerCategory1 = 12;

		private const int PackedShopPerCategory = 18;

		private const int PackedShopSpecialtyPerCategory = 40;

		// Sale / event / newbie / collab / PC-cafe style top tabs.
		private static readonly HashSet<int> PackedShopSpecialtyCategory1 = new HashSet<int>
		{
			50000, 60000, 70000, 90000, 120000, 130000, 190000, 200000
		};

		private static List<ShopDisplayItem> packedShopDisplayItems = new List<ShopDisplayItem>();

		private static HashSet<int> packedShopDisplayNums = new HashSet<int>();

		private static HashSet<int> packedShopSellItemNums = new HashSet<int>();

		public static IReadOnlyList<ShopDisplayItem> PackedShopDisplayItems => packedShopDisplayItems;

		public static HashSet<int> PackedShopDisplayNums => packedShopDisplayNums;

		public static HashSet<int> PackedShopSellItemNums => packedShopSellItemNums;

		public static void RebuildPackedShopCatalog()
		{
			List<ShopDisplayItem> nextItems = new List<ShopDisplayItem>();
			HashSet<int> nextNums = new HashSet<int>();
			HashSet<int> featured = new HashSet<int>();
			foreach (ShopCategoryPurchasing purchasing in ShopCategoryPurchasingList)
			{
				featured.Add(purchasing.ShopDisplayNum);
			}
			void AddItem(ShopDisplayItem item)
			{
				if (nextItems.Count >= PackedShopMaxTotal)
				{
					return;
				}
				if (nextNums.Add(item.ShopDisplayNum))
				{
					nextItems.Add(item);
				}
			}
			foreach (int displayNum in featured)
			{
				if (ShopDisplayItemListDict.TryGetValue(displayNum, out ShopDisplayItem featuredItem))
				{
					AddItem(featuredItem);
				}
			}
			// Guarantee items for every Category1 the category ACK lists (avoids empty tabs).
			foreach (int cat1 in ShopCategoryList.Select((ShopCategory c) => c.Category1).Distinct())
			{
				int have = 0;
				foreach (ShopDisplayItem existing in nextItems)
				{
					if (existing.Category1 == cat1)
					{
						have++;
					}
				}
				if (have >= PackedShopMinPerCategory1)
				{
					continue;
				}
				foreach (ShopDisplayItem item in ShopDisplayItemList.Where((ShopDisplayItem i) => i.Category1 == cat1).OrderBy((ShopDisplayItem i) => i.DisplaySortNum))
				{
					if (have >= PackedShopMinPerCategory1 || nextItems.Count >= PackedShopMaxTotal)
					{
						break;
					}
					int before = nextNums.Count;
					AddItem(item);
					if (nextNums.Count > before)
					{
						have++;
					}
				}
			}
			void TakeGroups(bool specialtyOnly)
			{
				foreach (IGrouping<string, ShopDisplayItem> group in ShopDisplayItemList.GroupBy((ShopDisplayItem c) => c.Category1 + "," + c.Category2 + "," + c.Category3))
				{
					if (nextItems.Count >= PackedShopMaxTotal)
					{
						break;
					}
					int cat1 = group.First().Category1;
					bool specialty = PackedShopSpecialtyCategory1.Contains(cat1);
					if (specialtyOnly != specialty)
					{
						continue;
					}
					int limit = specialty ? PackedShopSpecialtyPerCategory : PackedShopPerCategory;
					int taken = 0;
					foreach (ShopDisplayItem item2 in group.OrderBy((ShopDisplayItem c) => c.DisplaySortNum))
					{
						if (nextItems.Count >= PackedShopMaxTotal || taken >= limit)
						{
							break;
						}
						int before = nextNums.Count;
						AddItem(item2);
						if (nextNums.Count > before)
						{
							taken++;
						}
					}
				}
			}
			TakeGroups(specialtyOnly: true);
			TakeGroups(specialtyOnly: false);
			HashSet<int> nextSell = new HashSet<int>();
			foreach (ShopItemSell sell in ShopItemSellList.Values)
			{
				if (nextNums.Contains(sell.ShopDisplayNum))
				{
					nextSell.Add(sell.SellItemNum);
				}
			}
			packedShopDisplayItems = nextItems;
			packedShopDisplayNums = nextNums;
			packedShopSellItemNums = nextSell;
			Log.Information("Packed shop catalog display={0} sell={1} featured={2}", packedShopDisplayItems.Count, packedShopSellItemNums.Count, featured.Count);
		}


		public static void LoadShopInfo()
		{
			LoadSchedule();
			LoadNewShopInfo();
			LoadSelectivePackageInfo();
		}

		private static void LoadSchedule()
		{
			ShopActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new ShopManager()), "ShopManager");
			ServerStatus.QuartzActor.Tell(new CreateJob(ShopActor, 1, TriggerBuilder.Create().WithCronSchedule("2 0 0 ? * * *").Build()));
			ServerStatus.QuartzActor.Tell(new CreateJob(ShopActor, 2, TriggerBuilder.Create().WithCronSchedule("5 0 0 ? * * *").Build()));
		}

		public static void LoadNewShopInfo2()
		{
			try
			{
				ShopCategoryList.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essenshopcategory", mySqlConnection);
					mySqlCommand.Parameters.Clear();
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						ShopCategory item = new ShopCategory
						{
							Category1 = Convert.ToInt32(mySqlDataReader["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader["fdCategory3"]),
							CategoryName = mySqlDataReader["fdCategoryName"].ToString(),
							Category1Sub = Convert.ToInt32(mySqlDataReader["fdCategory1Sub"]),
							Category2Sub = Convert.ToInt32(mySqlDataReader["fdCategory2Sub"]),
							OriginCategory = Convert.ToInt32(mySqlDataReader["fdOriginCategory"])
						};
						ShopCategoryList.Add(item);
					}
				}
				ShopCategoryPurchasingList.Clear();
				using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection2.Open();
					using MySqlCommand mySqlCommand2 = new MySqlCommand("SELECT * FROM EssenShopCategoryPurchasing", mySqlConnection2);
					mySqlCommand2.Parameters.Clear();
					using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
					while (mySqlDataReader2.Read())
					{
						ShopCategoryPurchasing item2 = new ShopCategoryPurchasing
						{
							Category1 = Convert.ToInt32(mySqlDataReader2["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader2["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader2["fdCategory3"]),
							Order = Convert.ToInt32(mySqlDataReader2["fdOrder"]),
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader2["fdShopDisplayNum"])
						};
						ShopCategoryPurchasingList.Add(item2);
					}
				}
				ShopDisplayItemList.Clear();
				ShopDisplayItemListDict.Clear();
				using (MySqlConnection mySqlConnection3 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection3.Open();
					using MySqlCommand mySqlCommand3 = new MySqlCommand("SELECT * FROM essenshopdisplayitem", mySqlConnection3);
					mySqlCommand3.Parameters.Clear();
					using MySqlDataReader mySqlDataReader3 = mySqlCommand3.ExecuteReader();
					while (mySqlDataReader3.Read())
					{
						int num = Convert.ToInt32(mySqlDataReader3["fdShopDisplayNum"]);
						ShopDisplayItem shopDisplayItem = new ShopDisplayItem
						{
							ShopDisplayNum = num,
							Category1 = Convert.ToInt32(mySqlDataReader3["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader3["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader3["fdCategory3"]),
							DisplaySortNum = Convert.ToInt32(mySqlDataReader3["fdDisplaySortNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader3["fdItemNum"]),
							Gift = Convert.ToBoolean(mySqlDataReader3["fdGift"]),
							ItemTag = mySqlDataReader3["fdItemTag"].ToString(),
							Desc = mySqlDataReader3["fdDesc"].ToString()
						};
						ShopDisplayItemList.Add(shopDisplayItem);
						ShopDisplayItemListDict.Add(num, shopDisplayItem);
					}
				}
				ShopItemSellList.Clear();
				using (MySqlConnection mySqlConnection4 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection4.Open();
					using MySqlCommand mySqlCommand4 = new MySqlCommand("SELECT t1.fdSellItemNum, fdShopDisplayNum, \r\n                                                            t1.fdItemNum, fdOriginCostType, fdOriginPrice, \r\n                                                            IFNULL(t2.fdCostType, t1.fdCostType) AS fdCostType, \r\n                                                            IFNULL(t2.fdPrice, t1.fdPrice) AS fdPrice, \r\n                                                            fdMileageType, fdMileage, \r\n                                                            IFNULL(t2.fdShopDisplay, t1.fdShopDisplay) AS fdShopDisplay, \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tCASE WHEN t2.fdCostType IS NULL THEN 0 ELSE 1 END AS isEdit \r\n                                                        FROM EssenShopItemSelllist t1 LEFT Join EssenShopItemSelllist_2 t2 \r\n                                                        ON t1.fdSellItemNum = t2.fdSellItemNum;", mySqlConnection4);
					mySqlCommand4.Parameters.Clear();
					using MySqlDataReader mySqlDataReader4 = mySqlCommand4.ExecuteReader();
					while (mySqlDataReader4.Read())
					{
						int num2 = Convert.ToInt32(mySqlDataReader4["fdSellItemNum"]);
						ShopItemSell value = new ShopItemSell
						{
							SellItemNum = num2,
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader4["fdShopDisplayNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader4["fdItemNum"]),
							OriginCostType = Convert.ToByte(mySqlDataReader4["fdOriginCostType"]),
							OriginPrice = Convert.ToInt32(mySqlDataReader4["fdOriginPrice"]),
							CostType = Convert.ToByte(mySqlDataReader4["fdCostType"]),
							Price = Convert.ToInt32(mySqlDataReader4["fdPrice"]),
							MileageType = Convert.ToByte(mySqlDataReader4["fdMileageType"]),
							Mileage = Convert.ToSingle(mySqlDataReader4["fdMileage"]),
							ShopDisplay = Convert.ToBoolean(mySqlDataReader4["fdShopDisplay"]),
							isEdit = Convert.ToBoolean(mySqlDataReader4["isEdit"])
						};
						ShopItemSellList.Add(num2, value);
					}
				}
				ShopDisplayDateLimitList.Clear();
				using (MySqlConnection mySqlConnection5 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection5.Open();
					using MySqlCommand mySqlCommand5 = new MySqlCommand("SELECT * FROM essenshopdisplaydatelimit", mySqlConnection5);
					mySqlCommand5.Parameters.Clear();
					using MySqlDataReader mySqlDataReader5 = mySqlCommand5.ExecuteReader();
					while (mySqlDataReader5.Read())
					{
						int num3 = Convert.ToInt32(mySqlDataReader5["fdShopDisplayNum"]);
						ShopDisplayDateLimit value2 = new ShopDisplayDateLimit
						{
							ShopDisplayNum = num3,
							DisplayStartDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader5["fdDisplayStartDate"])),
							DisplayEndDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader5["fdDisplayEndDate"])),
							BuyStartDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader5["fdBuyStartDate"])),
							BuyEndDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader5["fdBuyEndDate"]))
						};
						ShopDisplayDateLimitList.Add(num3, value2);
					}
				}
				ShopBuyLimitCountList.Clear();
				using (MySqlConnection mySqlConnection6 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection6.Open();
					using MySqlCommand mySqlCommand6 = new MySqlCommand("SELECT * FROM essenshopbuylimitcount", mySqlConnection6);
					mySqlCommand6.Parameters.Clear();
					using MySqlDataReader mySqlDataReader6 = mySqlCommand6.ExecuteReader();
					while (mySqlDataReader6.Read())
					{
						ShopBuyLimitCount item3 = new ShopBuyLimitCount
						{
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader6["fdShopDisplayNum"]),
							DayPurchasingLimit = Convert.ToInt32(mySqlDataReader6["fdDayPurchasingLimit"]),
							MonthPurchasingLimit = Convert.ToInt32(mySqlDataReader6["fdMonthPurchasingLimit"]),
							TotalPurchasingLimit = Convert.ToInt32(mySqlDataReader6["fdTotalPurchasingLimit"]),
							AccountPurchasingLimit = mySqlDataReader6["fdAccountPurchasingLimit"] == DBNull.Value
								? 0
								: Convert.ToInt32(mySqlDataReader6["fdAccountPurchasingLimit"])
						};
						ShopBuyLimitCountList.Add(item3);
					}
				}
				ShopBuyAddBenefitList.Clear();
				using (MySqlConnection mySqlConnection7 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection7.Open();
					using MySqlCommand mySqlCommand7 = new MySqlCommand("SELECT * FROM essenshopbuyaddbenefit", mySqlConnection7);
					mySqlCommand7.Parameters.Clear();
					using MySqlDataReader mySqlDataReader7 = mySqlCommand7.ExecuteReader();
					while (mySqlDataReader7.Read())
					{
						ShopBuyAddBenefit item4 = new ShopBuyAddBenefit
						{
							SellItemNum = Convert.ToInt32(mySqlDataReader7["fdSellItemNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader7["fdItemNum"]),
							Count = Convert.ToInt32(mySqlDataReader7["fdCount"])
						};
						ShopBuyAddBenefitList.Add(item4);
					}
				}
				NotForSaleList.Clear();
				using (MySqlConnection mySqlConnection8 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection8.Open();
					using MySqlCommand mySqlCommand8 = new MySqlCommand("SELECT * FROM EssenShopNotForSale", mySqlConnection8);
					mySqlCommand8.Parameters.Clear();
					using MySqlDataReader mySqlDataReader8 = mySqlCommand8.ExecuteReader();
					while (mySqlDataReader8.Read())
					{
						NotForSaleList.Add(Convert.ToInt32(mySqlDataReader8["fdItemNum"]));
					}
				}
				ShopPurchasingLimitList.Clear();
				using (MySqlConnection mySqlConnection9 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection9.Open();
					using MySqlCommand mySqlCommand9 = new MySqlCommand("select fdShopDisplayNum, fdTotalPurchasingLimit \r\n                                                        from GameDataShopPurchasingLimit\r\n                                                        WHERE fdShopDisplayNum in (select fdShopDisplayNum \r\n                                                            from EssenShopBuyLimitCount WHERE fdTotalPurchasingLimit > 0)", mySqlConnection9);
					mySqlCommand9.Parameters.Clear();
					using MySqlDataReader mySqlDataReader9 = mySqlCommand9.ExecuteReader();
					while (mySqlDataReader9.Read())
					{
						GameDataShopPurchasingLimit item5 = new GameDataShopPurchasingLimit
						{
							ShopDisplayNum = mySqlDataReader9.GetInt32("fdShopDisplayNum"),
							TotalPurchasingLimit = mySqlDataReader9.GetInt32("fdTotalPurchasingLimit")
						};
						ShopPurchasingLimitList.Add(item5);
					}
				}
				ShopCategoryLimits.Clear();
				using (MySqlConnection mySqlConnection10 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection10.Open();
					using MySqlCommand mySqlCommand10 = new MySqlCommand("SELECT * FROM essenshopcategorylimit", mySqlConnection10);
					mySqlCommand10.Parameters.Clear();
					using MySqlDataReader mySqlDataReader10 = mySqlCommand10.ExecuteReader();
					while (mySqlDataReader10.Read())
					{
						ShopCategoryLimit item6 = new ShopCategoryLimit
						{
							CategoryNum = mySqlDataReader10.GetInt32("fdCategoryNum"),
							Type = mySqlDataReader10.GetByte("fdType"),
							OperatorType = mySqlDataReader10.GetByte("fdOperatorType"),
							Value = mySqlDataReader10.GetInt32("fdValue")
						};
						ShopCategoryLimits.Add(item6);
					}
				}
				Log.Information("Load NewShopInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load NewShopInfo Error: {0}", ex.Message);
			}
		}

		public static void LoadNewShopInfo()
		{
			try
			{
				ShopDisplayItemList.Clear();
				ShopDisplayItemListDict.Clear();
				ShopDisplayDateLimitList.Clear();
				ShopItemSellList.Clear();
				ShopBuyLimitCountList.Clear();
				ShopBuyAddBenefitList.Clear();
				ShopPurchasingLimitList.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_renewalShop_getDisplayList";
					mySqlCommand.Parameters.Add("CurrentDate", MySqlDbType.VarString).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int num = Convert.ToInt32(mySqlDataReader["fdShopDisplayNum"]);
						ShopDisplayItem shopDisplayItem = new ShopDisplayItem
						{
							ShopDisplayNum = num,
							Category1 = Convert.ToInt32(mySqlDataReader["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader["fdCategory3"]),
							DisplaySortNum = Convert.ToInt32(mySqlDataReader["fdDisplaySortNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
							Gift = Convert.ToBoolean(mySqlDataReader["fdGift"]),
							ItemTag = mySqlDataReader.GetString("fdItemTag"),
							Desc = mySqlDataReader.GetString("fdDesc"),
							isEdit = mySqlDataReader.GetBoolean("isEdit")
						};
						ShopDisplayItemList.Add(shopDisplayItem);
						ShopDisplayItemListDict.Add(num, shopDisplayItem);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						int num2 = Convert.ToInt32(mySqlDataReader["fdShopDisplayNum"]);
						ShopDisplayDateLimit value = new ShopDisplayDateLimit
						{
							ShopDisplayNum = num2,
							DisplayStartDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdDisplayStartDate"])),
							DisplayEndDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdDisplayEndDate"])),
							BuyStartDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdBuyStartDate"])),
							BuyEndDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdBuyEndDate"])),
							isEdit = mySqlDataReader.GetBoolean("isEdit")
						};
						ShopDisplayDateLimitList.Add(num2, value);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						int num3 = Convert.ToInt32(mySqlDataReader["fdSellItemNum"]);
						ShopItemSell value2 = new ShopItemSell
						{
							SellItemNum = num3,
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader["fdShopDisplayNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
							OriginCostType = Convert.ToByte(mySqlDataReader["fdOriginCostType"]),
							OriginPrice = Convert.ToInt32(mySqlDataReader["fdOriginPrice"]),
							CostType = Convert.ToByte(mySqlDataReader["fdCostType"]),
							Price = Convert.ToInt32(mySqlDataReader["fdPrice"]),
							MileageType = Convert.ToByte(mySqlDataReader["fdMileageType"]),
							Mileage = Convert.ToSingle(mySqlDataReader["fdMileage"]),
							ShopDisplay = Convert.ToBoolean(mySqlDataReader["fdShopDisplay"]),
							isEdit = Convert.ToBoolean(mySqlDataReader["isEdit"])
						};
						ShopItemSellList.Add(num3, value2);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						ShopBuyLimitCount item = new ShopBuyLimitCount
						{
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader["fdShopDisplayNum"]),
							DayPurchasingLimit = Convert.ToInt32(mySqlDataReader["fdDayPurchasingLimit"]),
							MonthPurchasingLimit = Convert.ToInt32(mySqlDataReader["fdMonthPurchasingLimit"]),
							TotalPurchasingLimit = Convert.ToInt32(mySqlDataReader["fdTotalPurchasingLimit"]),
							AccountPurchasingLimit = ReaderHasColumn(mySqlDataReader, "fdAccountPurchasingLimit") && mySqlDataReader["fdAccountPurchasingLimit"] != DBNull.Value
								? Convert.ToInt32(mySqlDataReader["fdAccountPurchasingLimit"])
								: 0,
							isEdit = mySqlDataReader.GetBoolean("isEdit")
						};
						ShopBuyLimitCountList.Add(item);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						ShopBuyAddBenefit item2 = new ShopBuyAddBenefit
						{
							SellItemNum = Convert.ToInt32(mySqlDataReader["fdSellItemNum"]),
							ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
							Count = Convert.ToInt32(mySqlDataReader["fdCount"]),
							isEdit = mySqlDataReader.GetBoolean("isEdit")
						};
						ShopBuyAddBenefitList.Add(item2);
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
					}
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						GameDataShopPurchasingLimit item3 = new GameDataShopPurchasingLimit
						{
							ShopDisplayNum = mySqlDataReader.GetInt32("fdShopDisplayNum"),
							TotalPurchasingLimit = mySqlDataReader.GetInt32("fdTotalPurchasingLimit")
						};
						ShopPurchasingLimitList.Add(item3);
					}
				}
				NotForSaleList.Clear();
				using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection2.Open();
					using MySqlCommand mySqlCommand2 = new MySqlCommand("SELECT * FROM EssenShopNotForSale", mySqlConnection2);
					mySqlCommand2.Parameters.Clear();
					using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
					while (mySqlDataReader2.Read())
					{
						NotForSaleList.Add(Convert.ToInt32(mySqlDataReader2["fdItemNum"]));
					}
				}
				ShopCategoryList.Clear();
				using (MySqlConnection mySqlConnection3 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection3.Open();
					using MySqlCommand mySqlCommand3 = new MySqlCommand("SELECT * FROM essenshopcategory", mySqlConnection3);
					mySqlCommand3.Parameters.Clear();
					using MySqlDataReader mySqlDataReader3 = mySqlCommand3.ExecuteReader();
					while (mySqlDataReader3.Read())
					{
						ShopCategory item4 = new ShopCategory
						{
							Category1 = Convert.ToInt32(mySqlDataReader3["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader3["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader3["fdCategory3"]),
							CategoryName = mySqlDataReader3["fdCategoryName"].ToString(),
							Category1Sub = Convert.ToInt32(mySqlDataReader3["fdCategory1Sub"]),
							Category2Sub = Convert.ToInt32(mySqlDataReader3["fdCategory2Sub"]),
							OriginCategory = Convert.ToInt32(mySqlDataReader3["fdOriginCategory"])
						};
						ShopCategoryList.Add(item4);
					}
				}
				ShopCategoryPurchasingList.Clear();
				using (MySqlConnection mySqlConnection4 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection4.Open();
					using MySqlCommand mySqlCommand4 = new MySqlCommand("SELECT * FROM EssenShopCategoryPurchasing", mySqlConnection4);
					mySqlCommand4.Parameters.Clear();
					using MySqlDataReader mySqlDataReader4 = mySqlCommand4.ExecuteReader();
					while (mySqlDataReader4.Read())
					{
						ShopCategoryPurchasing item5 = new ShopCategoryPurchasing
						{
							Category1 = Convert.ToInt32(mySqlDataReader4["fdCategory1"]),
							Category2 = Convert.ToInt32(mySqlDataReader4["fdCategory2"]),
							Category3 = Convert.ToInt32(mySqlDataReader4["fdCategory3"]),
							Order = Convert.ToInt32(mySqlDataReader4["fdOrder"]),
							ShopDisplayNum = Convert.ToInt32(mySqlDataReader4["fdShopDisplayNum"])
						};
						ShopCategoryPurchasingList.Add(item5);
					}
				}
				ShopCategoryLimits.Clear();
				using (MySqlConnection mySqlConnection5 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection5.Open();
					using MySqlCommand mySqlCommand5 = new MySqlCommand("SELECT * FROM essenshopcategorylimit", mySqlConnection5);
					mySqlCommand5.Parameters.Clear();
					using MySqlDataReader mySqlDataReader5 = mySqlCommand5.ExecuteReader();
					while (mySqlDataReader5.Read())
					{
						ShopCategoryLimit item6 = new ShopCategoryLimit
						{
							CategoryNum = mySqlDataReader5.GetInt32("fdCategoryNum"),
							Type = mySqlDataReader5.GetByte("fdType"),
							OperatorType = mySqlDataReader5.GetByte("fdOperatorType"),
							Value = mySqlDataReader5.GetInt32("fdValue")
						};
						ShopCategoryLimits.Add(item6);
					}
				}
				int edited = ShopDisplayItemList.Count((ShopDisplayItem c) => c.isEdit);
				if (edited > 200)
				{
					// Old KR tool wiped the whole catalog here and left packed shop empty.
					Log.Warning("ShopDisplayItemList isEdit count={0} (not clearing packed catalog)", edited);
				}
				RebuildPackedShopCatalog();
				Log.Information("Renewal Shop load OK");
			}
			catch (Exception ex)
			{
				Log.Error("Renewal Shop load Error: {0}", ex.Message);
			}
		}

		private static void LoadSelectivePackageInfo()
		{
			SelectivePackageList.Clear();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getSelectivePackage");
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					int @int = mySqlCommandHelper.GetInt("fdPackageItemNum");
					int int2 = mySqlCommandHelper.GetInt("fdBoxItemNum");
					if (SelectivePackageList.ContainsKey(@int))
					{
						SelectivePackageList[@int].Add(int2);
						continue;
					}
					SelectivePackageList.Add(@int, new List<int> { int2 });
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getSelectivePackag Error: {0}", ex.Message);
			}
		}

		private static bool ReaderHasColumn(IDataRecord reader, string columnName)
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}
