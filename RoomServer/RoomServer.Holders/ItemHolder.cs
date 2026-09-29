using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using RoomServer.Database;
using RoomServer.Structuring.Item;
using Serilog;
using TRCommon;
using Weighted_Randomizer;

namespace RoomServer.Holders
{
	public static class ItemHolder
	{
		public static NestedDictionary<ushort, byte, ushort, int> ItemPCKDict = new NestedDictionary<ushort, byte, ushort, int>();

		public static ConcurrentDictionary<int, ItemShopInfo> ItemShopInfos { get; } = new ConcurrentDictionary<int, ItemShopInfo>();


		public static ConcurrentDictionary<int, int> PetMaxEXP { get; } = new ConcurrentDictionary<int, int>();


		public static ConcurrentDictionary<int, bool> ExchangeSystemInfo { get; } = new ConcurrentDictionary<int, bool>();


		public static ConcurrentDictionary<int, List<int>> ItemSetDesc { get; } = new ConcurrentDictionary<int, List<int>>();


		public static ConcurrentDictionary<int, int> ItemSetByItemNum { get; } = new ConcurrentDictionary<int, int>();


		public static NestedDictionary<int, int, List<int>> ItemSetItemInfos { get; } = new NestedDictionary<int, int, List<int>>();


		public static NestedDictionary<int, int, List<ItemSetAttr>> ItemSetItemAttrInfos { get; } = new NestedDictionary<int, int, List<ItemSetAttr>>();


		public static ConcurrentDictionary<int, IWeightedRandomizer<short>> AlchemistEnchantData { get; set; } = new ConcurrentDictionary<int, IWeightedRandomizer<short>>();


		public static void LoadItemInfo()
		{
			Dictionary<int, ItemDataFromDatabase> dictionary = new Dictionary<int, ItemDataFromDatabase>();
			Dictionary<int, FarmItemDataFromDatabase> dictionary2 = new Dictionary<int, FarmItemDataFromDatabase>();
			Dictionary<int, CNetShuItemInfo> dictionary3 = new Dictionary<int, CNetShuItemInfo>();
			int num = -1;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getItemDescNum");
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					num = mySqlCommandHelper.GetInt("itemdescnum");
					if (!dictionary.ContainsKey(num))
					{
						dictionary.Add(num, new ItemDataFromDatabase());
					}
					dictionary[num].m_iItemDescNum = num;
					dictionary[num].m_iBillingItemID = mySqlCommandHelper.GetInt("billingitemid");
					dictionary[num].m_bShowInShop = mySqlCommandHelper.GetBoolean("showshop");
					dictionary[num].m_bPurchasable = mySqlCommandHelper.GetBoolean("purchasable");
					dictionary[num].m_eCoupleItemType = (eCoupleItemType)mySqlCommandHelper.GetInt("coupleItemType");
					dictionary[num].m_iType = mySqlCommandHelper.GetInt("type");
					dictionary[num].m_iCharacter = mySqlCommandHelper.GetInt("character");
					dictionary[num].m_iPosition = mySqlCommandHelper.GetInt("position");
					dictionary[num].m_iItemKind = mySqlCommandHelper.GetInt("kind");
					dictionary[num].m_iTRPrice = mySqlCommandHelper.GetInt("gamemoneyprice");
					dictionary[num].m_iCashPrice = mySqlCommandHelper.GetInt("cashprice");
					dictionary[num].m_iFarmPointPrice = mySqlCommandHelper.GetInt("farmpointprice");
					dictionary[num].m_iGuildPointPrice = mySqlCommandHelper.GetInt("guildpointprice");
					dictionary[num].m_iContributionPointPrice = mySqlCommandHelper.GetInt("contributionpointprice");
					dictionary[num].m_strItemName = mySqlCommandHelper.GetString("itemName");
					dictionary[num].m_iGotRateKind = mySqlCommandHelper.GetInt("getratekind");
					dictionary[num].m_iOnOffType = mySqlCommandHelper.GetInt("onofftype");
					dictionary[num].m_iSupplyItemDescNum = mySqlCommandHelper.GetInt("supplyItemDescNum");
					dictionary[num].m_iCategory = mySqlCommandHelper.GetInt("category");
					dictionary[num].m_iEtc = mySqlCommandHelper.GetInt("etc");
					dictionary[num].m_bRefundable = mySqlCommandHelper.GetBoolean("refundable");
					dictionary[num].m_bCanStack = mySqlCommandHelper.GetBoolean("canstack");
					dictionary[num].m_bNotDeleteWhenExpired = mySqlCommandHelper.GetBoolean("notDeleteWhenExpired");
					dictionary[num].m_bHasExpireTime = mySqlCommandHelper.GetBoolean("hasExpireTime");
					if (4 == dictionary[num].m_iType)
					{
						dictionary[num].m_packageItemNums.Add(dictionary[num].m_iSupplyItemDescNum);
					}
					if (!mySqlCommandHelper.IsDBNull("attrkey"))
					{
						short key = (short)mySqlCommandHelper.GetInt("attrkey");
						dictionary[num].m_mapAttr[key] = mySqlCommandHelper.GetFloat("attrvalue");
					}
					if (dictionary[num].m_iType == 1 || dictionary[num].m_iType == 2 || dictionary[num].m_iType == 4)
					{
						ItemCPK itemCPK = new ItemCPK
						{
							ItemChar = (byte)dictionary[num].m_iCharacter,
							ItemPosition = (ushort)dictionary[num].m_iPosition,
							ItemKind = (ushort)dictionary[num].m_iItemKind
						};
						ItemPCKDict[itemCPK.ItemPosition][itemCPK.ItemChar][itemCPK.ItemKind] = num;
					}
					ItemShopInfo value = new ItemShopInfo
					{
						Type = dictionary[num].m_iType,
						CanBuy = dictionary[num].m_bPurchasable,
						ItemPosition = (ushort)dictionary[num].m_iPosition,
						NotDeleteWhenExpired = dictionary[num].m_bNotDeleteWhenExpired,
						supplyItemDescNum = dictionary[num].m_iSupplyItemDescNum
					};
					ItemShopInfos.TryAdd(num, value);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getItemDescNum Error:{0} iItemDescNum:{1}", ex.ToString(), num);
			}
			Log.Information("Load ItemDescNum Count : {0}", dictionary.Count);
			try
			{
				using MySqlCommandHelper mySqlCommandHelper2 = new MySqlCommandHelper("usp_farmGetItemInfo");
				mySqlCommandHelper2.Execute();
				while (mySqlCommandHelper2.HasResult())
				{
					FarmItemDataFromDatabase farmItemDataFromDatabase = new FarmItemDataFromDatabase
					{
						m_iFarmItemNum = mySqlCommandHelper2.GetInt("farmItemDesc"),
						m_iAvatarItemNum = mySqlCommandHelper2.GetInt("avatarItemDescNum"),
						m_iPosition = mySqlCommandHelper2.GetInt("farmItemPosition")
					};
					dictionary2.Add(farmItemDataFromDatabase.m_iAvatarItemNum, farmItemDataFromDatabase);
				}
			}
			catch (Exception ex2)
			{
				Log.Error("usp_farmGetItemInfo Error: {0}", ex2.ToString());
			}
			Log.Information("Load FarmItem Count : {0}", dictionary2.Count);
			try
			{
				using MySqlCommandHelper mySqlCommandHelper3 = new MySqlCommandHelper("usp_shu_getItemInfo");
				mySqlCommandHelper3.Execute();
				while (mySqlCommandHelper3.HasResult())
				{
					CNetShuItemInfo cNetShuItemInfo = new CNetShuItemInfo(mySqlCommandHelper3.GetInt("avatarItemNum"), mySqlCommandHelper3.GetInt("avatarItemKind"), mySqlCommandHelper3.GetInt("character"), mySqlCommandHelper3.GetInt("position"), mySqlCommandHelper3.GetInt("kind"));
					dictionary3.Add(cNetShuItemInfo.getAvataritemNum, cNetShuItemInfo);
				}
			}
			catch (Exception ex3)
			{
				Log.Error("usp_shu_getItemInfo Error: {0}", ex3.ToString());
			}
			Log.Information("Load ShuItem Count : {0}", dictionary3.Count);
			ShopItemTable.onRecvItemDataFromDB(dictionary, dictionary2, dictionary3);
			LoadAlchemistData();
		}

		public static void LoadPetExpInfo()
		{
			try
			{
				foreach (string item in File.ReadLines("iteminfo\\essenmaxexpfrompetlevel.txt", Encoding.GetEncoding(1200)))
				{
					if (!item.StartsWith("//") && !item.StartsWith("_"))
					{
						MatchCollection matchCollection = new Regex("(?:^|,)((?:[^\",]|\"[^\"]*\")*)").Matches(item);
						PetMaxEXP.TryAdd(Convert.ToInt32(matchCollection[0].Groups[1].Value), Convert.ToInt32(matchCollection[1].Groups[1].Value));
					}
				}
				Log.Information("Load PetExpInfo Count: {0}", PetMaxEXP.Count());
			}
			catch (Exception ex)
			{
				Log.Error("Load PetExpInfo Error:{0}", ex.ToString());
			}
		}

		public static void LoadExchangeSystemInfo()
		{
			try
			{
				foreach (string item in File.ReadLines("iteminfo\\essenexchangesystemnameinfo.txt", Encoding.GetEncoding(1200)))
				{
					if (!item.StartsWith("//") && !item.StartsWith("_"))
					{
						MatchCollection matchCollection = new Regex("(?:^|,)((?:[^\",]|\"[^\"]*\")*)").Matches(item);
						ExchangeSystemInfo.TryAdd(Convert.ToInt32(matchCollection[0].Groups[1].Value), Convert.ToBoolean(matchCollection[2].Groups[1].Value));
					}
				}
				Log.Information("Load ExchangeSystemInfo Count: {0}", ExchangeSystemInfo.Count());
			}
			catch (Exception ex)
			{
				Log.Error("Load ExchangeSystemInfo Error:{0}", ex.ToString());
			}
		}

		public static void LoadItemSetInfo()
		{
			Dictionary<int, (short, List<SetMemberItemDesc>)> dictionary = new Dictionary<int, (short, List<SetMemberItemDesc>)>();
			Dictionary<int, Dictionary<int, Dictionary<short, float>>> dictionary2 = new Dictionary<int, Dictionary<int, Dictionary<short, float>>>();
			Dictionary<int, Dictionary<short, short>> dictionary3 = new Dictionary<int, Dictionary<short, short>>();
			try
			{
				foreach (string item2 in File.ReadLines("iteminfo\\tblavataritemsetdesc.txt", Encoding.GetEncoding(1200)))
				{
					if (item2.StartsWith("//") || item2.StartsWith("_"))
					{
						continue;
					}
					MatchCollection matchCollection = new Regex("(?:^|,)((?:[^\",]|\"[^\"]*\")*)").Matches(item2);
					int key = Convert.ToInt32(matchCollection[0].Groups[1].Value);
					SetMemberItemDesc item = default(SetMemberItemDesc);
					item.m_memberItemDescNum = Convert.ToInt32(matchCollection[1].Groups[1].Value);
					item.m_active = Convert.ToInt32(matchCollection[2].Groups[1].Value);
					int num = Convert.ToInt32(matchCollection[3].Groups[1].Value);
					if (dictionary.TryGetValue(key, out var value))
					{
						if (value.Item2 == null)
						{
							value.Item2 = new List<SetMemberItemDesc> { item };
						}
						else
						{
							value.Item2.Add(item);
						}
					}
					else
					{
						dictionary.Add(key, ((short)num, new List<SetMemberItemDesc> { item }));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getItemSetDesc Error:{0}", ex.ToString());
			}
			Log.Information("Load ItemSetDesc Count : {0}", dictionary.Count);
			try
			{
				foreach (string item3 in File.ReadLines("iteminfo\\tblavataritemsetattr.txt", Encoding.GetEncoding(1200)))
				{
					if (item3.StartsWith("//") || item3.StartsWith("_"))
					{
						continue;
					}
					MatchCollection matchCollection2 = new Regex("(?:^|,)((?:[^\",]|\"[^\"]*\")*)").Matches(item3);
					int key2 = Convert.ToInt32(matchCollection2[1].Groups[1].Value);
					int key3 = Convert.ToInt32(matchCollection2[2].Groups[1].Value);
					short key4 = Convert.ToInt16(matchCollection2[3].Groups[1].Value);
					float value2 = Convert.ToSingle(matchCollection2[4].Groups[1].Value);
					short value3 = Convert.ToInt16(matchCollection2[5].Groups[1].Value);
					if (dictionary2.ContainsKey(key2))
					{
						if (dictionary2[key2].ContainsKey(key3))
						{
							if (dictionary2[key2][key3].ContainsKey(key4))
							{
								dictionary2[key2][key3][key4] = value2;
							}
							else
							{
								dictionary2[key2][key3].Add(key4, value2);
							}
						}
						else
						{
							dictionary2[key2].Add(key3, new Dictionary<short, float> { { key4, value2 } });
						}
					}
					else
					{
						dictionary2.Add(key2, new Dictionary<int, Dictionary<short, float>> { 
						{
							key3,
							new Dictionary<short, float> { { key4, value2 } }
						} });
					}
					if (dictionary3.ContainsKey(key2))
					{
						if (dictionary3[key2].ContainsKey(key4))
						{
							dictionary3[key2][key4] = value3;
						}
						else
						{
							dictionary3[key2].Add(key4, value3);
						}
					}
					else
					{
						dictionary3.Add(key2, new Dictionary<short, short> { { key4, value3 } });
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Error("usp_getItemSetAttr Error:{0}", ex2.ToString());
			}
			SetItemDescTableEx.onRecvData(dictionary);
			SetItemDescTableEx.onRecvSetAttrApplyTargets(dictionary3);
			SetItemAttrTable.reloadSetItemAttrTable(dictionary2);
		}

		public static void LoadItemTransformInfo()
		{
			Dictionary<(int, int, int), CItemTransformInfo> dictionary = new Dictionary<(int, int, int), CItemTransformInfo>();
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getItemTransformInfo"))
				{
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						CItemTransformInfo cItemTransformInfo = new CItemTransformInfo
						{
							m_iCharacter = mySqlCommandHelper.GetUShort("character"),
							m_iPosition = mySqlCommandHelper.GetUShort("position"),
							m_iOriginKind = mySqlCommandHelper.GetUShort("originKind"),
							m_iTransKind = mySqlCommandHelper.GetUShort("transKind"),
							m_transformType = (eItemTransformType)mySqlCommandHelper.GetShort("transType"),
							m_strValue = mySqlCommandHelper.GetString("value")
						};
						(cpk_type, cpk_type, cpk_type) tuple = (cItemTransformInfo.m_iCharacter, cItemTransformInfo.m_iPosition, cItemTransformInfo.m_iTransKind);
						(cpk_type, cpk_type, cpk_type) tuple2 = tuple;
						if (!dictionary.ContainsKey((tuple2.Item1, tuple2.Item2, tuple2.Item3)))
						{
							tuple2 = tuple;
							dictionary.Add((tuple2.Item1, tuple2.Item2, tuple2.Item3), cItemTransformInfo);
						}
					}
				}
				TransformItemManager.onRecvItemTransformInfoFromDB(dictionary);
				Log.Information("Load ItemTransformInfo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load ItemTransformInfo Error:{0}", ex.ToString());
			}
		}

		private static void LoadAlchemistData()
		{
			AlchemistEnchantData.Clear();
			NestedDictionary<int, short, int> nestedDictionary = new NestedDictionary<int, short, int>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_alchemist_LoadData";
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						int @int = mySqlDataReader.GetInt32("ItemNum");
						int int2 = mySqlDataReader.GetInt32("Ratio");
						short int3 = mySqlDataReader.GetInt16("Class");
						nestedDictionary.Add(@int, int3, int2);
					}
				}
				foreach (KeyValuePair<int, NestedDictionary<short, int>> item in nestedDictionary)
				{
					IWeightedRandomizer<short> weightedRandomizer = new StaticWeightedRandomizer<short>();
					foreach (KeyValuePair<short, int> item2 in item.Value)
					{
						weightedRandomizer.Add(item2.Key, item2.Value);
					}
					AlchemistEnchantData.TryAdd(item.Key, weightedRandomizer);
				}
				Log.Information("Load AlchemistData Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load AlchemistData Error:{0}", ex.Message);
			}
		}
	}
}
