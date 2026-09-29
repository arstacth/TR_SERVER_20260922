using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class ShopHandle
	{
		public static void Handle_BuyItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			List<ShopBuyItemInfo> list = new List<ShopBuyItemInfo>();
			eShopFailed_REASON failedReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
			for (int i = 0; i < num2; i++)
			{
				int num3 = reader.ReadLEInt32();
				int unk = reader.ReadLEInt32();
				reader.ReadLEInt64();
				reader.ReadLEInt32();
				int unk2 = reader.ReadLEInt32();
				reader.ReadLEInt64();
				reader.Offset += 20;
				int num4 = reader.ReadLEInt32();
				if (num == 4020 && ShopHolder.ShopItemSellList.TryGetValue(num4, out var sellRow) && sellRow.ItemNum > 0 && sellRow.ItemNum != num3)
				{
					Log.Warning("Shop sell-id remap user={0} clientItem={1} sell={2} serverItem={3}", currentAccount.UserNum, num3, num4, sellRow.ItemNum);
					num3 = sellRow.ItemNum;
				}
				ShopBuyItemInfo shopBuyItemInfo = new ShopBuyItemInfo
				{
					ItemNum = num3,
					unk3 = unk,
					unk4 = unk2,
					SellItemNum = num4
				};
				ItemBillingContents contents;
				bool itemBillingContentsFromItemDescNum = ShopItemTable.getItemBillingContentsFromItemDescNum(shopBuyItemInfo.ItemNum, out contents);
				bool flag = num == 4020 || ShopItemTable.isUserCanBuyOrGiftItem(shopBuyItemInfo.ItemNum);
				bool flag2 = (itemBillingContentsFromItemDescNum || num == 4020) && flag && !ShopHolder.NotForSaleList.Contains(num3);
				if (num == 4020)
				{
					if (ShopItemTable.getRealItemDataFromItemDescNum(num3, out var itemData4020))
					{
						shopBuyItemInfo.ItemPosition = itemData4020.m_iPosition;
					}
				}
				else if (!(itemBillingContentsFromItemDescNum && flag2))
				{
					shopBuyItemInfo.NotForSale = true;
					Log.Warning("NotForSale OR ItemInfo Error ItemNum:{0} fdSellItemNum:{1}", num3, num4);
				}
				else
				{
					ShopItemTable.getRealItemDataFromItemDescNum(num3, out var itemData);
					shopBuyItemInfo.ItemPosition = itemData.m_iPosition;
				}
				list.Add(shopBuyItemInfo);
			}
			if (list.All((ShopBuyItemInfo a) => a.NotForSale))
			{
				Client.SendAsync(new ShopBuyItemFail_New(failedReason, list, last));
				return;
			}
			switch (num)
			{
			case 4000:
			{
				if (!list.All((ShopBuyItemInfo a) => a.ItemPosition == 1060))
				{
					Log.Warning("Use OldShop Hack!!! NickName:{0}, ItemNum:{1}", currentAccount.NickName, list.FirstOrDefault().ItemNum);
					Client.SendAsync(new ShopBuyItemFail_New(eShopFailed_REASON.eShopFailed_REASON_UNKNOWN, list, last));
					break;
				}
				long guildpoint2 = 0L;
				int contributionpoint2 = 0;
				eShopFailed_REASON m_failReason2 = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				foreach (ShopBuyItemInfo item in list.Where((ShopBuyItemInfo w) => !w.NotForSale))
				{
					if (BuyItemCheck(currentAccount, item.ItemNum, item.SellItemNum, num, out guildpoint2, out contributionpoint2, out m_failReason2))
					{
						item.BuySuccess = true;
					}
				}
				if (list.Count((ShopBuyItemInfo c) => c.BuySuccess) > 0)
				{
					ItemHandle.PrepareBoughtItems(list);
					if (list.All((ShopBuyItemInfo a) => a.BuySuccess))
					{
						Client.SendAsync(new ShopBuyItem_ACK(list, isFarmOrShuItem: false, last));
					}
					else
					{
						Client.SendAsync(new ShopBuyItemFail_New(m_failReason2, list, last));
					}
					currentAccount.CashNeedUpdateFromDB = true;
					ItemHandle.NotifyBoughtItems(currentAccount, list, last);
					if (list.Any((ShopBuyItemInfo a) => a.BuySuccess && a.ItemPosition == 0))
					{
						Client.SendAsync(new MyRoom_GetCharacterList(currentAccount, last));
					}
					Client.SendAsync(new GuildGetContributionPointACK(contributionpoint2, last));
					Client.SendAsync(new GuildGetPointACK(guildpoint2, last));
				}
				break;
			}
			case 4005:
			case 4020:
			{
				long guildpoint = 0L;
				int contributionpoint = 0;
				eShopFailed_REASON m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				foreach (ShopBuyItemInfo item2 in list.Where((ShopBuyItemInfo w) => !w.NotForSale))
				{
					if (BuyItemCheck(currentAccount, item2.ItemNum, item2.SellItemNum, num, out guildpoint, out contributionpoint, out m_failReason))
					{
						item2.BuySuccess = true;
					}
				}
				if (list.Count((ShopBuyItemInfo c) => c.BuySuccess) > 0)
				{
					ItemHandle.PrepareBoughtItems(list);
					if (list.All((ShopBuyItemInfo a) => a.BuySuccess))
					{
						Client.SendAsync(new ShopBuyItem_ACK(list, isFarmOrShuItem: false, last));
					}
					else
					{
						Client.SendAsync(new ShopBuyItemFail_New(m_failReason, list, last));
					}
					if (list.Any((ShopBuyItemInfo a) => a.ItemPosition == 115 || a.ItemPosition == 116))
					{
						Client.SendAsync(new ShopBuyFreePassUpdate(currentAccount, last));
						Client.SendAsync(new ShopBuyFreePassUpdate2(currentAccount, last));
					}
					currentAccount.CashNeedUpdateFromDB = true;
					ItemHandle.NotifyBoughtItems(currentAccount, list, last);
					if (list.Any((ShopBuyItemInfo a) => a.BuySuccess && a.ItemPosition == 0))
					{
						Client.SendAsync(new MyRoom_GetCharacterList(currentAccount, last));
					}
					if (num == 4005)
					{
						Client.SendAsync(new GuildGetContributionPointACK(contributionpoint, last));
						Client.SendAsync(new GuildGetPointACK(guildpoint, last));
					}
				}
				else
				{
					Client.SendAsync(new ShopBuyItemFail_New(m_failReason, list, last));
				}
				break;
			}
			case 4012:
			{
				string text = string.Empty;
				eShopFailed_REASON m_failReason3 = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				foreach (ShopBuyItemInfo item3 in list.Where((ShopBuyItemInfo w) => !w.NotForSale))
				{
					if (BuyShuItemCheck(currentAccount, item3.ItemNum, out var itemid2, out m_failReason3))
					{
						item3.BuySuccess = true;
						item3.ItemID = itemid2;
						item3.supplyItemDescNum = item3.ItemNum;
						text += $"{itemid2},";
					}
				}
				if (list.Count((ShopBuyItemInfo c) => c.BuySuccess) > 0)
				{
					ShuSystemHandle.Shu_GetUserItemInfo(currentAccount, text, string.Empty, out var iteminfos);
					Client.SendAsync(new ShopBuyItem_ACK(list, isFarmOrShuItem: true, last));
					ItemHandle.getActiveFuncItem(currentAccount, -1);
					Client.SendAsync(new Shu_BuyItemOK2(iteminfos, last));
				}
				else
				{
					Client.SendAsync(new ShopBuyItemFail_New(m_failReason3, list, last));
				}
				break;
			}
			case 4001:
				foreach (ShopBuyItemInfo item4 in list.Where((ShopBuyItemInfo w) => !w.NotForSale))
				{
					if (BuyFarmItemCheck(currentAccount, item4.ItemNum, out var itemid, out var ItemDescNum))
					{
						item4.BuySuccess = true;
						item4.ItemID = itemid;
						item4.supplyItemDescNum = ItemDescNum;
					}
				}
				if (list.Count((ShopBuyItemInfo c) => c.BuySuccess) > 0)
				{
					Client.SendAsync(new ShopBuyItem_ACK(list, isFarmOrShuItem: true, last));
				}
				else
				{
					Client.SendAsync(new ShopBuyItemFail_New(eShopFailed_REASON.eShopFailed_REASON_UNKNOWN, list, last));
				}
				break;
			case 4006:
				Client.SendAsync(new ShopBuyItemFail_New(eShopFailed_REASON.eShopFailed_REASON_UNKNOWN, list, last));
				break;
			default:
				Log.Warning("Unknown ShopType:{0}", num);
				Client.SendAsync(new ShopBuyItemFail_New(eShopFailed_REASON.eShopFailed_REASON_UNKNOWN, list, last));
				break;
			}
		}

		public static void Handle_GiftItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt16();
			string memo = string.Empty;
			if (num2 > 0)
			{
				memo = reader.ReadBig5StringSafe(num2) ?? string.Empty;
			}
			reader.ReadLEInt32();
			int num3 = reader.ReadLEInt32();
			int num4 = reader.ReadLEInt32();
			ItemBillingContents contents;
			bool itemBillingContentsFromItemDescNum = ShopItemTable.getItemBillingContentsFromItemDescNum(num, out contents);
			bool flag = num3 == 4020 || ShopItemTable.isUserCanBuyOrGiftItem(num);
			eGiftItemFailed failedReason = eGiftItemFailed.eGiftItemFailed_UNKNOWN;
			if (num3 == 4000)
			{
				Log.Warning("Use OldShop[Gift] Hack!!! SendNickName:{0} RecvNickName:{1}", currentAccount.NickName, text);
			}
			else if (num3 == 4001)
			{
				if (ShopItemTable.isFarmItem(num))
				{
					goto IL_012c;
				}
				Log.Warning("Use OldShop[Gift] Hack!!! SendNickName:{0} RecvNickName:{1}", currentAccount.NickName, text);
			}
			else if (num3 == 4020)
			{
				if (ShopHolder.ShopItemSellList.TryGetValue(num4, out var value) && value.ItemNum == num)
				{
					goto IL_012c;
				}
				failedReason = eGiftItemFailed.eGiftItemFailed_NO_RENWAL_SHOP_ITEM_INVALID;
				Log.Warning("!!!!Shop Item Hack Or UserData Not Same as Server UserNum:{0}, ItemNum:{1}, SellItemNum:{2}", currentAccount.UserNum, num, num4);
			}
			else
			{
				Log.Warning("Unknown ShopType:{0}", num3);
			}
			goto IL_019c;
			IL_019c:
			Client.SendAsync(new ShopGiftItemError_ACK(failedReason, last));
			return;
			IL_012c:
			if (!itemBillingContentsFromItemDescNum || !flag || ShopHolder.NotForSaleList.Contains(num) || currentAccount.Level < ServerSettingHolder.ServerSettings.GiftItemLimitLevel)
			{
				failedReason = eGiftItemFailed.eGiftItemFailed_CANNOT_GIFT;
				goto IL_019c;
			}
			if (GiftItemCheck(currentAccount, num, text, memo, num4, num3, out var m_failReason))
			{
				Client.SendAsync(new ShopGiftItemOK_ACK(currentAccount, num, text, last));
			}
			else
			{
				Client.SendAsync(new ShopGiftItemError_ACK(m_failReason, last));
			}
		}

		public static void Handle_GetCurrentGameMoney(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			// Always reload from DB. Gating on TRNeedUpdateFromDB left the HUD on a stale
			// LOGIN_OK value (e.g. 781k) while UserInfoGame was already 281k.
			if (currentAccount != null)
			{
				getCurrentGameMoney(currentAccount);
			}
			Client.SendAsync(new CurrentGameMoney_ACK(currentAccount, last));
		}

		/// <summary>Reload TR from UserInfoGame into account memory.</summary>
		public static void RefreshGameMoney(Account User)
		{
			if (User == null)
			{
				return;
			}
			getCurrentGameMoney(User);
		}

		public static void Handle_GetShopCategoryDisplayItem(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int cat1 = reader.ReadLEInt32();
			int cat2 = reader.ReadLEInt32();
			int cat3 = reader.ReadLEInt32();
			List<ShopCategoryPurchasing> list = (from w in ShopHolder.ShopCategoryPurchasingList
				where w.Category1 == cat1 && w.Category2 == cat2 && w.Category3 == cat3
				select w into o
				orderby o.Order
				select o).ToList();
			if (list.Count == 0)
			{
				list = (from w in ShopHolder.PackedShopDisplayItems
					where w.Category1 == cat1 && w.Category2 == cat2 && w.Category3 == cat3
					orderby w.DisplaySortNum
					select new ShopCategoryPurchasing
					{
						Category1 = w.Category1,
						Category2 = w.Category2,
						Category3 = w.Category3,
						Order = w.DisplaySortNum,
						ShopDisplayNum = w.ShopDisplayNum
					}).ToList();
			}
			Client.SendAsync(new GetShopCategoryDisplayItem(cat1, cat2, cat3, list, last));
		}

		public static void Handle_GetShopCategoryList(ClientConnection Client, byte last)
		{
			Client.SendAsync(new GetShopCategoryList(last));
		}

		public static void Handle_GetShopDisplayList(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new GetShopDisplayItemList(last));
			Client.SendAsync(new GetShopItemSellList(last));
			Client.SendAsync(new GetShopDisplayDateLimitList(last));
			Client.SendAsync(new GetShopBuyLimitCountList(last));
			Client.SendAsync(new GetShopBuyAddBenefitList(last));
			Client.SendAsync(new GetShopBuyAddSellPriceList(last));
		}

		public static void Handle_GetUserVip(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			renewalShop_getUserVipLevel(currentAccount);
			Client.SendAsync(new GetShopVipLevelNotify(currentAccount.VipLevel, last));
			renewalShop_getMileage(currentAccount);
		}

		public static void Handle_GetUserBuyList(ClientConnection Client, PacketReader reader, byte last)
		{
			renewalShop_getUserBuyList(Client.CurrentAccount, out var UserBuyCountList, out var ShopPurchasingLimit);
			Client.SendAsync(new GetShopUserBuyList_ACK(UserBuyCountList, ShopPurchasingLimit, last));
		}

		public static void Handle_GetShopPurchasingLimitList(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			List<int> itemlist = new List<int>();
			int num = reader.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				itemlist.Add(reader.ReadLEInt32());
			}
			IEnumerable<GameDataShopPurchasingLimit> shopPurchasingLimit = ShopHolder.ShopPurchasingLimitList.Where((GameDataShopPurchasingLimit w) => itemlist.Contains(w.ShopDisplayNum));
			Client.SendAsync(new GetShopPurchasingLimitList_ACK(shopPurchasingLimit, last));
		}

		public static void Handle_OpenSelectivePackage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			List<ShopBuyItemInfo> buyitemOKlist = new List<ShopBuyItemInfo>();
			string text = string.Empty;
			for (int i = 0; i < num2; i++)
			{
				int num3 = reader.ReadLEInt32();
				reader.ReadLEInt32();
				reader.ReadLEInt64();
				reader.ReadLEInt32();
				reader.ReadLEInt32();
				reader.ReadLEInt64();
				reader.Offset += 20;
				reader.ReadLEInt32();
				if (ShopHolder.SelectivePackageList.TryGetValue(num, out var value) && value.Contains(num3))
				{
					text += $"{num3},";
					continue;
				}
				Client.SendAsync(new ShopBuyItemFail_New(eShopFailed_REASON.eShopFailed_REASON_INVALID_PAYMENT_TYPE, buyitemOKlist, last));
				return;
			}
			if (openSelectivePackage(currentAccount, num, text, out var m_failReason, out var infos))
			{
				Client.SendAsync(new Shop_OpenSelectivePackage_ACK(result: true, infos, num, m_failReason, last));
			}
			else
			{
				Client.SendAsync(new Shop_OpenSelectivePackage_ACK(result: false, infos, num, m_failReason, last));
			}
		}

		public static bool BuyItemCheck(Account User, int itemid, int SellItemNum, int shopKind, out long guildpoint, out int contributionpoint, out eShopFailed_REASON m_failReason)
		{
			guildpoint = 0L;
			contributionpoint = 0;
			m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_shopBuyProductItemDescNum_New";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemid;
					mySqlCommand.Parameters.Add("SellItemNum", MySqlDbType.Int32).Value = SellItemNum;
					mySqlCommand.Parameters.Add("shopKind", MySqlDbType.Int32).Value = shopKind;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					int buyRet = Convert.ToInt32(mySqlDataReader["ret"]);
					if (buyRet == 0)
					{
						mySqlDataReader.GetInt32("CostType");
						User.TR = Convert.ToInt64(mySqlDataReader["gamemoney"]);
						User.Cash -= Convert.ToInt32(mySqlDataReader["CashPrice"]);
						guildpoint = Convert.ToInt64(mySqlDataReader["guildpoint"]);
						contributionpoint = Convert.ToInt32(mySqlDataReader["contributionpoint"]);
						return true;
					}
					Log.Warning("shop buy ret={0} user={1} item={2} sell={3} kind={4}", buyRet, User.UserNum, itemid, SellItemNum, shopKind);
				}
				return false;
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("already in gift list"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_IN_GIFT_LIST;
				}
				else if (ex.Message.Contains("already have pet"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("no character for the item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_CHARACTER_FOR_THE_ITEM;
				}
				else if (ex.Message.Contains("Duplicate entry"))
				{
					if (TryCompleteCharacterBuyAfterDuplicate(User, itemid))
					{
						Log.Information("shop character grant after setting duplicate user={0} item={1}", User.UserNum, itemid);
						return true;
					}
					if (UserOwnsCharacterItem(User, itemid))
					{
						Log.Information("shop already owns character user={0} item={1}", User.UserNum, itemid);
						return true;
					}
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("already have item") || ex.Message.Contains("duplicate item") || ex.Message.Contains("check error"))
				{
					if (UserOwnsCharacterItem(User, itemid))
					{
						Log.Information("shop already owns character user={0} item={1}", User.UserNum, itemid);
						return true;
					}
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("not enough necessary item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_ENOUGH_NECESSARY_ITEM;
				}
				else if (ex.Message.Contains("not enough farm level"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_ENOUGH_LEVEL;
				}
				else if (ex.Message.Contains("enough add friend item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ENOUGHT_ADDFRIEND_ITEM;
				}
				else if (ex.Message.Contains("couple level is too low"))
				{
					m_failReason = eShopFailed_REASON.eShopfailed_REASON_NOT_ENOUGH_COUPLE_LEVEL;
				}
				else if (ex.Message.Contains("you are not couple"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_COUPLE;
				}
				else if (ex.Message.Contains("already have item of same position"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_DUP_POSITION_ITEM;
				}
				else if (ex.Message.Contains("already married"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_MARRIED;
				}
				else if (ex.Message.Contains("shu item inventory is full"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_SHU_INVENTORY_FULL;
				}
				else if (ex.Message.Contains("not enough limit count"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_RENWAL_SHOP_ITEM_INVALID_COUNT;
				}
				else if (ex.Message.Contains("not enough limit level"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_RENWAL_SHOP_ITEM_INVALID_LEVEL;
				}
				else if (ex.Message.Contains("unknown costtype"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_RENWAL_SHOP_ITEM_INVALID_PAYMENT;
				}
				else
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_GAME_SERVER_PROBLEM;
				}
				Log.Error("usp_shopBuyProductItemDescNum_New Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				Log.Error("usp_shopBuyProductItemDescNum_New error: {0}", ex2.Message);
			}
			return false;
		}

		private static bool GiftItemCheck(Account User, int itemid, string nickname, string memo, int SellItemNum, int shopKind, out eGiftItemFailed m_failReason)
		{
			m_failReason = eGiftItemFailed.eGiftItemFailed_UNKNOWN;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_shopGiftItem_New";
					mySqlCommand.Parameters.Add("sendUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("sendNickname", MySqlDbType.VarChar).Value = User.NickName;
					mySqlCommand.Parameters.Add("receiveNickname", MySqlDbType.VarChar).Value = nickname;
					mySqlCommand.Parameters.Add("itemDescNum", MySqlDbType.Int32).Value = itemid;
					mySqlCommand.Parameters.Add("memo", MySqlDbType.VarChar).Value = string.IsNullOrEmpty(memo) ? string.Empty : memo;
					mySqlCommand.Parameters.Add("SellItemNum", MySqlDbType.Int32).Value = SellItemNum;
					mySqlCommand.Parameters.Add("shopKind", MySqlDbType.Int32).Value = shopKind;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToByte(mySqlDataReader["ret"]) == 0)
					{
						switch (mySqlDataReader.GetInt32("CostType"))
						{
						case 0:
							User.TR -= Convert.ToInt32(mySqlDataReader["gamemoney"]);
							break;
						case 1:
							User.Cash -= Convert.ToInt32(mySqlDataReader["cash"]);
							break;
						}
						return true;
					}
				}
				return false;
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("invalid receive nickname") || ex.Message.Contains("invalid user"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_INVALID_NICKNAME;
				}
				else if (ex.Message.Contains("already have") || ex.Message.Contains("duplicate item") || ex.Message.Contains("already have pet"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_ALREADY_HAVE;
				}
				else if (ex.Message.Contains("already gifted"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_ALREADY_GIFT;
				}
				else if (ex.Message.Contains("no character for the item"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NO_CHARACTER_HAVE;
				}
				else if (ex.Message.Contains("not enough farm level"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NOT_ENOUGH_LEVEL;
				}
				else if (ex.Message.Contains("enough add friend item"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_ENOUGHT_ADDFRIEND_ITEM;
				}
				else if (ex.Message.Contains("there is no CoupleInfo"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NOT_COUPLE;
				}
				else if (ex.Message.Contains("couple level is too low"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NOT_ENOUGH_COUPLE_LEVEL;
				}
				else if (ex.Message.Contains("cannot gift self"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_CANNOT_SELF;
				}
				else if (ex.Message.Contains("not enough limit level"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NOT_ENOUGH_LEVEL_LIMIT;
				}
				else if (ex.Message.Contains("can not gift item"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_CANNOT_GIFT;
				}
				else if (ex.Message.Contains("not enough limit count"))
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_NO_RENWAL_SHOP_ITEM_INVALID_COUNT;
				}
				else
				{
					m_failReason = eGiftItemFailed.eGiftItemFailed_UNKNOWN;
				}
				Log.Error("usp_shopGiftItem_New Error:{0} {1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "");
			}
			catch (Exception ex2)
			{
				m_failReason = eGiftItemFailed.eGiftItemFailed_UNKNOWN;
				Log.Error("usp_shopGiftItem_New error: {0}", ex2.Message);
			}
			return false;
		}

		private static bool BuyShuItemCheck(Account User, int itemnum, out long itemid, out eShopFailed_REASON m_failReason)
		{
			itemid = 0L;
			m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_shu_buyItem";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemnum;
					mySqlCommand.Parameters.Add("paymentType", MySqlDbType.Int32).Value = 1;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						itemid = Convert.ToInt64(mySqlDataReader["itemID"]);
						User.TR -= Convert.ToInt32(mySqlDataReader["gameMoneyPrice"]);
						User.Cash -= Convert.ToInt32(mySqlDataReader["CashPrice"]);
						return true;
					}
				}
				return false;
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("already in gift list"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_IN_GIFT_LIST;
				}
				else if (ex.Message.Contains("already have pet"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("no character for the item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_CHARACTER_FOR_THE_ITEM;
				}
				else if (ex.Message.Contains("already have item") || ex.Message.Contains("duplicate item") || ex.Message.Contains("check error"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_HAVE_ITEM;
				}
				else if (ex.Message.Contains("not enough necessary item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_ENOUGH_NECESSARY_ITEM;
				}
				else if (ex.Message.Contains("not enough farm level"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_ENOUGH_LEVEL;
				}
				else if (ex.Message.Contains("enough add friend item"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ENOUGHT_ADDFRIEND_ITEM;
				}
				else if (ex.Message.Contains("couple level is too low"))
				{
					m_failReason = eShopFailed_REASON.eShopfailed_REASON_NOT_ENOUGH_COUPLE_LEVEL;
				}
				else if (ex.Message.Contains("you are not couple"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_COUPLE;
				}
				else if (ex.Message.Contains("already have item of same position"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_DUP_POSITION_ITEM;
				}
				else if (ex.Message.Contains("already married"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_ALREADY_MARRIED;
				}
				else if (ex.Message.Contains("shu item inventory is full"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_SHU_INVENTORY_FULL;
				}
				else if (ex.Message.Contains("not enough limit count"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_RENWAL_SHOP_ITEM_INVALID_COUNT;
				}
				else if (ex.Message.Contains("not enough limit level"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NO_RENWAL_SHOP_ITEM_INVALID_LEVEL;
				}
				else
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				}
				Log.Error("usp_shu_buyItem Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				Log.Error("usp_shu_buyItem error: {0}", ex2.Message);
			}
			return false;
		}

		private static bool BuyFarmItemCheck(Account User, int itemnum, out long itemid, out int ItemDescNum)
		{
			ItemDescNum = 0;
			itemid = 0L;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_BuyProductItemDescNum";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemnum;
					mySqlCommand.Parameters.Add("paymentType", MySqlDbType.Int32).Value = 1;
					mySqlCommand.Parameters.Add("discountCouponList", MySqlDbType.VarString).Value = 1;
					mySqlCommand.Parameters.Add("farmItemID", MySqlDbType.Int64).Value = -1;
					mySqlCommand.Parameters.Add("shopKind", MySqlDbType.Int32).Value = 0;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						User.TR = Convert.ToInt64(mySqlDataReader["gamemoney"]);
						User.Cash -= Convert.ToInt32(mySqlDataReader["CashPrice"]);
						itemid = Convert.ToInt64(mySqlDataReader["ItemID"]);
						ItemDescNum = Convert.ToInt32(mySqlDataReader["ItemDescNum"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_BuyProductItemDescNum error: {0}", ex.Message);
				return false;
			}
		}

		private static bool UserOwnsCharacterItem(Account User, int itemid)
		{
			if (!ShopItemTable.getItemDataFromItemDescNum(itemid, out var itemData) || itemData.m_iType != 1 || itemData.m_iPosition != 0 || itemData.m_iItemKind != 0 || itemData.m_iCharacter == 0)
			{
				return false;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand owned = new MySqlCommand("SELECT COUNT(*) FROM tblAvatarUserCharacter WHERE fdUserNum=@u AND fdItemDescNum=@i", mySqlConnection);
				owned.Parameters.AddWithValue("@u", User.UserNum);
				owned.Parameters.AddWithValue("@i", itemid);
				return Convert.ToInt32(owned.ExecuteScalar()) > 0;
			}
			catch (Exception ex)
			{
				Log.Error("UserOwnsCharacterItem user={0} item={1}: {2}", User.UserNum, itemid, ex.Message);
				return false;
			}
		}

		private static bool TryCompleteCharacterBuyAfterDuplicate(Account User, int itemid)
		{
			if (!ShopItemTable.getItemDataFromItemDescNum(itemid, out var itemData) || itemData.m_iType != 1 || itemData.m_iPosition != 0 || itemData.m_iItemKind != 0 || itemData.m_iCharacter == 0)
			{
				return false;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using (MySqlCommand owned = new MySqlCommand("SELECT COUNT(*) FROM tblAvatarUserCharacter WHERE fdUserNum=@u AND fdItemDescNum=@i", mySqlConnection))
				{
					owned.Parameters.AddWithValue("@u", User.UserNum);
					owned.Parameters.AddWithValue("@i", itemid);
					if (Convert.ToInt32(owned.ExecuteScalar()) > 0)
					{
						return false;
					}
				}
				using (MySqlCommand insChar = new MySqlCommand("INSERT INTO tblAvatarUserCharacter (fdUserNum, fdItemDescNum, fdCharacter, fdCount, fdUsing, fdGotDateTime) VALUES (@u, @i, @c, 1, 1, NOW())", mySqlConnection))
				{
					insChar.Parameters.AddWithValue("@u", User.UserNum);
					insChar.Parameters.AddWithValue("@i", itemid);
					insChar.Parameters.AddWithValue("@c", itemData.m_iCharacter);
					insChar.ExecuteNonQuery();
				}
				using MySqlCommand insSet = new MySqlCommand("INSERT IGNORE INTO tblAvatarCharacterSetting (fdUserNum, fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot, fdACHead, fdACHand, fdACFace, fdACBack, fdACNeck, fdPet, fdExpansion, fdACWrist, fdACBooster, fdPart) VALUES (@u, @c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)", mySqlConnection);
				insSet.Parameters.AddWithValue("@u", User.UserNum);
				insSet.Parameters.AddWithValue("@c", itemData.m_iCharacter);
				insSet.ExecuteNonQuery();
				using (MySqlCommand insCpk = new MySqlCommand("INSERT IGNORE INTO EssenAvatarItemCPKRef (fdItemNum, fdChar, fdPos, fdKind) VALUES (@i, @c, 0, 0)", mySqlConnection))
				{
					insCpk.Parameters.AddWithValue("@i", itemid);
					insCpk.Parameters.AddWithValue("@c", itemData.m_iCharacter);
					insCpk.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("TryCompleteCharacterBuyAfterDuplicate user={0} item={1}: {2}", User.UserNum, itemid, ex.Message);
				return false;
			}
		}

		public static void shopGetGiftAcceptWaitList(int UserNum, int startindex, int lastindex, out List<UserGiftInfo> GiftList, out int Itemcount)
		{
			GiftList = new List<UserGiftInfo>();
			Itemcount = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_shopGetGiftAcceptWaitList";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("startIndex", MySqlDbType.Int32).Value = startindex;
				mySqlCommand.Parameters.Add("lastIndex", MySqlDbType.Int32).Value = lastindex;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					UserGiftInfo item = new UserGiftInfo
					{
						uniNum = mySqlDataReader.GetInt32("uniNum"),
						sendNickname = mySqlDataReader.GetString("sendNickname"),
						itemDescNum = mySqlDataReader.GetInt32("itemDescNum"),
						sendDateTime = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("sendDateTime")),
						memo = mySqlDataReader.GetString("memo"),
						Expire = (Convert.IsDBNull(mySqlDataReader["Expire"]) ? (-1) : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("Expire")))
					};
					Itemcount = mySqlDataReader.GetInt32("Itemcount");
					GiftList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_shopGetGiftAcceptWaitList UserNum:{0}, error: {1}", UserNum, ex.Message);
			}
		}

		public static void storage_getKeepingItemList(int UserNum, out List<UserStorageItemInfo> KeepList)
		{
			KeepList = new List<UserStorageItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_storage_getKeepingItemList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				List<UserStorageItemAttr> list = new List<UserStorageItemAttr>();
				while (mySqlDataReader.Read())
				{
					UserStorageItemAttr item = new UserStorageItemAttr
					{
						uniqueNum = mySqlDataReader.GetInt64("uniqueNum"),
						type = mySqlDataReader.GetInt16("type"),
						value = mySqlDataReader.GetFloat("value")
					};
					list.Add(item);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					UserStorageItemInfo info = new UserStorageItemInfo
					{
						uniqueNum = mySqlDataReader.GetInt64("uniqueNum"),
						itemNum = mySqlDataReader.GetInt32("itemNum"),
						dateTime = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("dateTime"))
					};
					if (!list.Exists((UserStorageItemAttr e) => e.uniqueNum == info.uniqueNum && e.type == 2 && e.value == 1f))
					{
						KeepList.Add(info);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_storage_getKeepingItemList error: {0}", ex.Message);
			}
		}

		public static void storage_getGiftList(int UserNum, out List<UserStorageItemInfo> GiftList)
		{
			GiftList = new List<UserStorageItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_storage_getGiftList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					UserStorageItemInfo item = new UserStorageItemInfo
					{
						uniqueNum = mySqlDataReader.GetInt64("uniqueNum"),
						itemNum = mySqlDataReader.GetInt32("itemNum"),
						dateTime = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("dateTime")),
						sendNickname = mySqlDataReader.GetString("sendNickname"),
						memo = mySqlDataReader.GetString("memo")
					};
					GiftList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_storage_getGiftList error: {0}", ex.Message);
			}
		}

		private static void getCurrentGameMoney(Account User)
		{
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getCurrentGameMoney"))
				{
					mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
					mySqlCommandHelper.ExecuteSingle();
					if (mySqlCommandHelper.HasResult())
					{
						User.TR = mySqlCommandHelper.GetLong("gamemoney");
					}
				}
				User.TRNeedUpdateFromDB = false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_getCurrentGameMoney Error:{0}", ex.Message);
			}
		}

		private static void renewalShop_getUserBuyList(Account User, out List<ShopBuyCountList> UserBuyCountList, out List<GameDataShopPurchasingLimit> ShopPurchasingLimit)
		{
			UserBuyCountList = new List<ShopBuyCountList>();
			ShopPurchasingLimit = new List<GameDataShopPurchasingLimit>();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_renewalShop_getUserBuyList");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamDateTime("toDayDate", DateTime.Now);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
				}
				mySqlCommandHelper.NextResult();
				while (mySqlCommandHelper.HasResult())
				{
					ShopBuyCountList item = new ShopBuyCountList
					{
						ShopDisplayNum = mySqlCommandHelper.GetInt("fdShopDisplayNum"),
						DayPurchasingLimit = mySqlCommandHelper.GetInt("fdDayPurchasingLimit"),
						MonthPurchasingLimit = mySqlCommandHelper.GetInt("fdMonthPurchasingLimit"),
						TotalPurchasingLimit = mySqlCommandHelper.GetInt("fdTotalPurchasingLimit")
					};
					UserBuyCountList.Add(item);
				}
				mySqlCommandHelper.NextResult();
				while (mySqlCommandHelper.HasResult())
				{
					GameDataShopPurchasingLimit item2 = new GameDataShopPurchasingLimit
					{
						ShopDisplayNum = mySqlCommandHelper.GetInt("fdShopDisplayNum"),
						TotalPurchasingLimit = mySqlCommandHelper.GetInt("fdTotalPurchasingLimit")
					};
					ShopPurchasingLimit.Add(item2);
				}
				// Do NOT dump EssenShopBuyLimitCount here — 30k+ rows → ~250KB ACK and client crash.
				// Client already has Essen max limits; server only needs user buy counts.
				ShopHolder.ShopPurchasingLimitList = ShopPurchasingLimit;
			}
			catch (Exception ex)
			{
				Log.Error("usp_renewalShop_getUserBuyList Error:{0}", ex.Message);
			}
		}

		private static void renewalShop_getMileage(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_renewalShop_getMileage");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					User.MileagePoint = mySqlCommandHelper.GetInt("iTotalMileage");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_renewalShop_getMileage Error:{0}", ex.Message);
			}
		}

		private static void renewalShop_getUserVipLevel(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_renewalShop_getUserVipLevel");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					User.VipLevel = mySqlCommandHelper.GetInt("vipLevel");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_renewalShop_getUserVipLevel Error:{0}", ex.Message);
			}
		}

		private static bool openSelectivePackage(Account User, int iPackageItemNum, string BuyingItem_ETC, out eShopFailed_REASON m_failReason, out List<ExchangeItemInfo> infos)
		{
			infos = new List<ExchangeItemInfo>();
			m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_openSelectivePackage");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pItemDescNum", iPackageItemNum);
				mySqlCommandHelper.AddParamVarString("pBoxItemNums_ETC", BuyingItem_ETC);
				mySqlCommandHelper.AddParamVarString("pBoxItemNums_CASH", string.Empty);
				mySqlCommandHelper.Execute();
				long tR = User.TR;
				long exp = User.Exp;
				while (mySqlCommandHelper.HasResult())
				{
					ExchangeItemInfo item = new ExchangeItemInfo
					{
						type = mySqlCommandHelper.GetInt("rewardType"),
						id = mySqlCommandHelper.GetInt("rewardID"),
						count = mySqlCommandHelper.GetInt("amount")
					};
					exp = mySqlCommandHelper.GetLong("totalExp");
					tR = mySqlCommandHelper.GetLong("totalGameMoney");
					infos.Add(item);
				}
				User.TR = tR;
				User.Exp = exp;
				return true;
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("not enought item 1"))
				{
					m_failReason = eShopFailed_REASON.eShopFailed_REASON_NOT_ENOUGH_NECESSARY_ITEM;
				}
				Log.Error("usp_openSelectivePackage Error: {0}", ex.Message);
			}
			catch (Exception ex2)
			{
				Log.Error("usp_openSelectivePackage Error:{0}", ex2.Message);
			}
			return false;
		}
	}
}
