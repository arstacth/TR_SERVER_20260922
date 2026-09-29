using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AgentServer;
using AgentServer.Database;
using AgentServer.Function;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class LobbyHandle
	{
		public static void Handle_ShowPage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte type = reader.ReadByte();
			currentAccount.TRNeedUpdateFromDB = true;
			currentAccount.EXPNeedUpdateFromDB = true;
			currentAccount.CashNeedUpdateFromDB = true;
			Client.SendAsync(new ShowPage(currentAccount, type, last));
		}

		public static void HandlePingTime(ClientConnection Client, int type, byte last)
		{
			Account User = Client.CurrentAccount;
			IEnumerable<Account> enumerable = ClientConnection.CurrentAccounts.Values.Where((Account players) => players.UserNum == User.UserNum && players.isLogin);
			if (User.UserNum > 0 && enumerable.Count() > 1)
			{
				Log.Information("User [{0}] has already logged in!", User.UserID);
				Client.SendAsync(new LoginError(6, last, 0));
				{
					foreach (Account item in enumerable)
					{
						if (item.Session != User.Session)
						{
							item.Connection.SendAsync(new DisconnectPacket(259, last));
							item.Connection.Disconnect(5000);
						}
					}
					return;
				}
			}
			long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			Client.SendAsync(new PingTime(User.bLogin, time, last));
			User.checkActiveItem(last);
		}

		public static void Handle_SinglePlay(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			bool allow = num > 0;
			if (MapHolder.MapInfos.TryGetValue(num, out var value))
			{
				allow = value.CanTimeAttack;
			}
			else
			{
				// Classic maps (5xxx) may be missing from tblMapInfo until
				// patch_client_maps_missing.sql is applied; still ACK so AloneRun starts.
				Log.Warning("ALONERUN_START map={0} user={1} not in MapHolder; sending ACK anyway", num, currentAccount != null ? currentAccount.UserID : "?");
				allow = true;
			}
			if (allow)
			{
				Client.SendAsync(new SinglePlay(num, last));
				currentAccount.SinglePlayMapNum = num;
			}
			else
			{
				Log.Warning("ALONERUN_START map={0} user={1} rejected (CanTimeAttack=false)", num, currentAccount != null ? currentAccount.UserID : "?");
			}
		}

		public static void Handle_GetUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			TBDyeInfo(text, out var AvatarItemDyeing);
			FishingHandle.GetFishRecord(text, out var fishrecord);
			Client.SendAsync(new GetUserInfo(currentAccount, text, AvatarItemDyeing, fishrecord, last));
		}

		public static void Handle_GetUserPoint(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			// debug_trgame: "%d Shu MP" is point type 0x834 (2100). Without an ACK the
			// HUD stays at the unset sentinel (-1). Map to UserShuInfo.fdMP.
			if (num == 2100)
			{
				int mp = ReadShuMpPoint(currentAccount.UserNum);
				Client.SendAsync(new GetUserPoint(num, mp, mp, last));
				return;
			}
			requestUserPoint(currentAccount.UserNum, num, out var totlapoint, out var currentpoint);
			Client.SendAsync(new GetUserPoint(num, totlapoint, currentpoint, last));
		}

		public static void Handle_eServer_GET_ITEM_COLLECTION_INFO_REQ(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			bool bOtherUser = currentAccount.NickName != text;
			if (itemCollection_GetUserInfo(text, bOtherUser, 1, out var info, out var _))
			{
				Client.SendAsync(new eServer_GET_ITEM_COLLECTION_INFO_REQ(text, bOtherUser, info, last));
			}
		}

		public static void Handle_eServer_ITEM_COLLECTION_USER_LIST_REQ(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			bool bOtherUser = currentAccount.NickName != text;
			if (itemCollection_GetUserInfo(text, bOtherUser, 2, out var _, out var itemnums))
			{
				Client.SendAsync(new eServer_ITEM_COLLECTION_USER_LIST_REQ(text, itemnums, last));
			}
		}

		public static void Handle_eServer_ITEM_COLLECTION_ADD_REQ(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			if (new Regex("^[\\d,]+$").IsMatch(text) && itemCollection_UdateUserInfo(currentAccount.UserNum, 2, text))
			{
				Client.SendAsync(new eServer_ITEM_COLLECTION_ADD_ACK(last));
			}
		}

		public static void Handle_GetGameOption(ClientConnection Client, byte last)
		{
			int option = Client.CurrentAccount != null ? Client.CurrentAccount.GameOption : 0;
			// Packed GET_ACK (wire 780) is required to finish login. Skipping it on
			// first-login made the client report "Login timeout with loginok" and
			// never show character select. Pink LogoIntro is a side effect of this ACK.
			if (Conf.ProtocolDebug)
			{
				Log.Information("GAME_OPTION_GET_REQ user={0} option={1} last={2} ack=wire780 GET_ACK", Client.CurrentAccount != null ? Client.CurrentAccount.UserID : "?", option, last);
			}
			Client.SendAsync(new GameOptionGetAck(option, last));
		}

		public static void Handle_ItemCollectionProtocol(ClientConnection Client, PacketReader reader, byte last)
		{
			int inner = 0;
			int theme = 0;
			if (reader.Remaining >= 4)
			{
				inner = reader.ReadLEInt32();
			}
			if (reader.Remaining >= 4)
			{
				theme = reader.ReadLEInt32();
			}
			if (Conf.ProtocolDebug)
			{
				Log.Information("ItemCollection_PROTOCOL user={0} inner={1} theme={2}", Client.CurrentAccount != null ? Client.CurrentAccount.UserID : "?", inner, theme);
			}
			// REQ inner 4 is 7 bytes (no theme) = level-up / month reward claim.
			// Answering with USER_INFO (3) reopens LuckyBagItemCollectionLevelUpReward.gui.
			if (inner == 4)
			{
				Account accReward = Client.CurrentAccount;
				int userNumReward = accReward != null ? accReward.UserNum : 0;
				SyncItemCollectionNoticedLevel(userNumReward, 1, 1, 1, 0);
				Client.SendAsync(new PackedItemCollectionRewardAck(last));
				return;
			}
			// Packed dbgtrace: classic 683/625 are Unknown (not consumed).
			// Bare one-int ACK is treated as invalid inner → UI stays at 99999.
			// Open (inner 2 / theme0) needs USER_INFO_ACK inner3.
			if (theme <= 0 || inner == 2 || inner == 3)
			{
				Account acc = Client.CurrentAccount;
				int rank = 0;
				int point = 0;
				byte noticed = 0;
				int released = 0;
				List<int> itemnums = new List<int>();
				string nick = (acc != null) ? acc.NickName : null;
				int userNum = (acc != null) ? acc.UserNum : 0;
				if (acc != null && !string.IsNullOrEmpty(nick)
					&& itemCollection_GetUserInfo(nick, bOtherUser: false, 1, out var info, out var _))
				{
					rank = info.rank;
					point = info.point;
					noticed = info.noticedLevel;
					released = info.releasedItemCount;
				}
				if (acc != null && !string.IsNullOrEmpty(nick))
				{
					itemCollection_GetUserInfo(nick, bOtherUser: false, 2, out var _, out itemnums);
				}
				if (itemnums == null || itemnums.Count == 0)
				{
					itemnums = GetOwnedRenewalCollectionItems(userNum);
				}
				if (released <= 0 && itemnums != null)
				{
					released = itemnums.Count;
				}
				// Prefer leaderboard rank when SP/rank row is missing.
				if (rank <= 0 && userNum > 0)
				{
					rank = GetItemCollectionLeaderboardRank(userNum, nick);
				}
				if (Conf.ProtocolDebug)
				{
					Log.Information("ItemCollection_PROTOCOL theme0 point={0} rank={1} released={2} owned={3} noticed={4}", point, rank, released, itemnums.Count, noticed);
				}
				// Stale fdNoticedLevel (e.g. 1) with real points opens
				// LuckyBagItemCollectionLevelUpReward.gui (purple) with no items.
				noticed = SyncItemCollectionNoticedLevel(userNum, point, released, rank, noticed);
				byte wireNoticed = GetMaxCollectionLevel();
				if (wireNoticed == 0)
				{
					wireNoticed = 255;
				}
				if (noticed > wireNoticed)
				{
					wireNoticed = noticed;
				}
				Client.SendAsync(new PackedItemCollectionUserInfoAck(userNum, point, point, point, released, wireNoticed, rank, last));
				return;
			}
			int bookValue = CountOwnedCollectionTheme(Client.CurrentAccount != null ? Client.CurrentAccount.UserNum : 0, theme);
			if (Conf.ProtocolDebug)
			{
				Log.Information("ItemCollection_PROTOCOL bookValue={0} theme={1}", bookValue, theme);
			}
			Client.SendAsync(new PackedItemCollectionValueAck(bookValue, last));
		}

		private static byte _maxCollectionLevel;
		private static bool _maxCollectionLevelLoaded;

		/// <summary>
		/// Keep noticedLevel at max defined collection level so login does not pop
		/// LuckyBagItemCollectionLevelUpReward.gui (purple) with empty reward slots.
		/// Always persist — stale DB noticed below max re-opens the purple UI.
		/// </summary>
		private static byte SyncItemCollectionNoticedLevel(int userNum, int point, int released, int rank, byte noticed)
		{
			if (point <= 0 && released <= 0 && rank <= 0 && noticed == 0)
			{
				return noticed;
			}
			byte target = GetMaxCollectionLevel();
			if (target == 0)
			{
				target = 255;
			}
			if (noticed >= target)
			{
				return noticed;
			}
			if (userNum > 0)
			{
				try
				{
					using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
					conn.Open();
					using MySqlCommand cmd = new MySqlCommand(
						"UPDATE UserItemCollection SET fdNoticedLevel=@n WHERE fdUserNum=@u",
						conn);
					cmd.Parameters.AddWithValue("@n", (int)target);
					cmd.Parameters.AddWithValue("@u", userNum);
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Log.Warning("SyncItemCollectionNoticedLevel user={0}: {1}", userNum, ex.Message);
				}
			}
			if (Conf.ProtocolDebug)
			{
				Log.Information("ItemCollection noticed sync {0} -> {1} point={2}", noticed, target, point);
			}
			return target;
		}

		private static byte GetMaxCollectionLevel()
		{
			if (_maxCollectionLevelLoaded)
			{
				return _maxCollectionLevel;
			}
			_maxCollectionLevelLoaded = true;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT IFNULL(MAX(fdLevel),0) FROM essenitem_collectionrenewal_level",
					conn);
				object o = cmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					int v = Convert.ToInt32(o);
					_maxCollectionLevel = (byte)Math.Min(255, Math.Max(0, v));
				}
			}
			catch (Exception ex)
			{
				Log.Warning("GetMaxCollectionLevel: {0}", ex.Message);
				_maxCollectionLevel = 255;
			}
			return _maxCollectionLevel;
		}

		private static List<int> GetOwnedRenewalCollectionItems(int userNum)
		{
			return GetOwnedRenewalCollectionIds(userNum);
		}

		private static int GetItemCollectionLeaderboardRank(int userNum, string nick)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT fdRank FROM GameDataItemCollectionRank WHERE fdUserNum=@u OR fdNickName=@n ORDER BY fdRank LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", userNum);
				mySqlCommand.Parameters.AddWithValue("@n", nick ?? string.Empty);
				object o = mySqlCommand.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					return Convert.ToInt32(o);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("GetItemCollectionLeaderboardRank: {0}", ex.Message);
			}
			return 0;
		}

		/// <summary>
		/// Packed MapAddItemNotify CollectionNum is essenitem_collectionrenewal_itemlist.fdID.
		/// </summary>
		private static List<int> GetOwnedRenewalCollectionIds(int userNum)
		{
			List<int> list = new List<int>();
			if (userNum <= 0)
			{
				return list;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT DISTINCT i.fdID FROM essenitem_collectionrenewal_itemlist i INNER JOIN tblavataruser u ON u.fdItemDescNum = i.fdItemNum WHERE u.fdUserNum = @u AND i.fdUse = 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", userNum);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					list.Add(mySqlDataReader.GetInt32(0));
				}
			}
			catch (Exception ex)
			{
				Log.Warning("GetOwnedRenewalCollectionIds user={0}: {1}", userNum, ex.Message);
			}
			return list;
		}

		private static int CountOwnedCollectionTheme(int userNum, int theme)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT COUNT(*) FROM essenitem_collectionrenewal_itemlist i INNER JOIN tblavataruser u ON u.fdItemDescNum = i.fdItemNum WHERE u.fdUserNum = @u AND i.fdType = @t AND i.fdUse = 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", userNum);
				mySqlCommand.Parameters.AddWithValue("@t", theme);
				object obj = mySqlCommand.ExecuteScalar();
				if (obj == null || obj == DBNull.Value)
				{
					return 0;
				}
				return Convert.ToInt32(obj);
			}
			catch (Exception ex)
			{
				Log.Warning("CountOwnedCollectionTheme user={0} theme={1}: {2}", userNum, theme, ex.Message);
				return 0;
			}
		}

		public static void Handle_SetGameOption(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (reader.Remaining >= 32)
			{
				if (currentAccount != null)
				{
					currentAccount.PackedOptionDumpUtc = DateTime.UtcNow;
				}
				Client.SendAsync(new PackedEmptyAck(1816, last, 0, writeLast: false));
				return;
			}
			int num = reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			bool flag2 = false;
			flag2 = ((!flag) ? FlagsHelper.IsSet(currentAccount.GameOption, num) : (!FlagsHelper.IsSet(currentAccount.GameOption, num)));
			if (flag2 && currentAccount.GameOption >= 0)
			{
				int flags = currentAccount.GameOption;
				if (flag)
				{
					FlagsHelper.Set(ref flags, num);
				}
				else
				{
					FlagsHelper.Unset(ref flags, num);
				}
				setGameOption(currentAccount, flags);
				Client.SendAsync(new SetGameOption(currentAccount.GameOption, last));
			}
			else
			{
				Log.Warning("[GameOption Invalid Set] UserNum:{0} CurrentOption:{1} SetValue:{2} isAdd:{3}", currentAccount.UserNum, currentAccount.GameOption, num, flag);
			}
		}

		public static void Handle_eServer_GET_EXP_REQ(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short num = reader.ReadLEInt16();
			long value = 0L;
			if (num == 1)
			{
				value = currentAccount.Exp;
			}
			if (num == 1 && currentAccount.EXPNeedUpdateFromDB)
			{
				int level = currentAccount.Level;
				getExp(currentAccount, num);
				if (LevelUPCheck(currentAccount, level))
				{
					Client.SendAsync(new UserLevelUPEXPInfo(1, currentAccount.Level, currentAccount.Exp, last));
				}
				value = currentAccount.Exp;
			}
			Client.SendAsync(new eServer_GET_EXP_REQ(num, value, last));
		}

		public static void Handle_SetHotKey(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt16();
			string text = string.Empty;
			List<int> list = new List<int>();
			for (int i = 0; i < num; i++)
			{
				reader.ReadLEInt16();
				int num2 = reader.ReadLEInt32();
				list.Add(num2);
				text = ((i == 9) ? (text + $"{num2}") : (text + $"{num2},"));
			}
			if (HotKeySet(currentAccount.UserNum, text))
			{
				Client.SendAsync(new HotKeySetOK(list, last));
			}
		}

		public static void Handle_GetHotKey(ClientConnection Client, byte last)
		{
			HotKeyGet(Client.CurrentAccount.UserNum, out var hkstr);
			List<int> list = new List<int>();
			if (!string.IsNullOrEmpty(hkstr))
			{
				string[] array = hkstr.Split(',');
				foreach (string value in array)
				{
					list.Add(Convert.ToInt32(value));
				}
			}
			Client.SendAsync(new HotKeyGetOK(list, last));
		}

		public static void Handle_CardPackOpen(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int cardpacknum = reader.ReadLEInt32();
			short opentype = reader.ReadLEInt16();
			if (OpenCardPack(currentAccount, cardpacknum, opentype, out var cardpackinfos))
			{
				Client.SendAsync(new CardPackOpen(cardpacknum, opentype, cardpackinfos, last));
			}
		}

		public static void Handle_SinglePlayGoalResult(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int singlePlayMapNum = currentAccount.SinglePlayMapNum;
			int num = reader.ReadLEInt32();
			int num2 = 0;
			if (MapHolder.MapInfos.TryGetValue(singlePlayMapNum, out var value) && value != null)
			{
				num2 = value.GoalInLimitTime * 1000;
			}
			else
			{
				Log.Warning("ALONERUN_GOAL map={0} user={1} not in MapHolder; skipping anti-cheat check", singlePlayMapNum, currentAccount != null ? currentAccount.NickName : "?");
			}
			if (num2 != 0 && num < num2)
			{
				Log.Warning("Player[{0}] {1}ms goalin {2} map too fast in single play mode!", currentAccount.NickName, num, singlePlayMapNum);
				GMCommandHandle.AutoBan(currentAccount.NickName, 1, 10, 0, currentAccount.LastIp);
				Client.SendAsync(new DisconnectPacket(260, last));
				Client.Disconnect(5000);
			}
			else
			{
				Client.SendAsync(new SinglePlayGoal_ACK(num, last));
			}
		}

		public static void Handle_GetOfficialCompetitionOpenTime(ClientConnection Client, PacketReader reader, byte last)
		{
			reader.ReadLEInt32();
			Client.SendAsync(new GetOfficialCompetitionOpenTime_ACK(last));
		}

		public static bool LevelUPCheck(Account User, int beforelevel)
		{
			User.GetMyLevel();
			bool num = User.Level != beforelevel;
			if (num && User.Level >= 71 && (new List<int> { 71, 78, 85 }.Contains(User.Level) || User.Level >= 92))
			{
				ActorRefImplicitSenderExtensions.Tell(message: new LevelUPNotice($"{User.NickName},{User.Level}", 1), receiver: ServerStatus.LBServerActor);
			}
			return num;
		}

		public static bool itemCollection_GetUserInfo(string UserName, bool bOtherUser, short reqType, out UserItemCollectionInfo info, out List<int> itemnums)
		{
			info = new UserItemCollectionInfo();
			itemnums = new List<int>();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemCollection_GetUserInfo");
				mySqlCommandHelper.AddParamVarString("nickName", UserName);
				mySqlCommandHelper.AddParamShort("bOtherUser", (short)(bOtherUser ? 1 : 0));
				mySqlCommandHelper.AddParamShort("reqType", reqType);
				mySqlCommandHelper.Execute();
				switch (reqType)
				{
				case 1:
					if (mySqlCommandHelper.HasResult())
					{
						info.point = mySqlCommandHelper.GetInt("point");
						info.rank = mySqlCommandHelper.GetInt("rank");
						info.noticedLevel = mySqlCommandHelper.GetByteConvert("noticedLevel");
						try
						{
							info.releasedItemCount = mySqlCommandHelper.GetInt("releasedItemCount");
						}
						catch
						{
							info.releasedItemCount = 0;
						}
						return true;
					}
					info.point = 0;
					info.rank = 0;
					info.noticedLevel = 0;
					info.releasedItemCount = 0;
					return true;
				case 2:
					if (mySqlCommandHelper.HasRows)
					{
						while (mySqlCommandHelper.HasResult())
						{
							itemnums.Add(mySqlCommandHelper.GetInt("itemNum"));
						}
						return true;
					}
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemCollection_GetUserInfo Error: {0}", ex.Message);
			}
			return false;
		}

		private static bool itemCollection_UdateUserInfo(int UserNun, short reqType, string val)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemCollection_UpdateUserInfo");
				mySqlCommandHelper.AddParamInt("userNum", UserNun);
				mySqlCommandHelper.AddParamShort("reqType", reqType);
				mySqlCommandHelper.AddParamVarString("val", val);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					if (mySqlCommandHelper.GetInt("ret") == 0)
					{
						ServerSettingHolder.RebuildItemCollectionRank();
						return true;
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemCollection_UpdateUserInfo Error: {0}", ex.Message);
			}
			return false;
		}

		private static void setGameOption(Account User, int optionvalue)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_setGameOption");
				mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("option", optionvalue);
				mySqlCommandHelper.ExecuteNonQuery();
				User.GameOption = optionvalue;
			}
			catch (Exception ex)
			{
				Log.Error("usp_setGameOption Error: {0}", ex.Message);
			}
		}

		private static void requestUserPoint(int UserNum, int rewardGroup, out int totlapoint, out int currentpoint)
		{
			totlapoint = 0;
			currentpoint = 0;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_requestUserPoint");
				mySqlCommandHelper.AddParamInt("userNum", UserNum);
				mySqlCommandHelper.AddParamInt("rewardGroup", rewardGroup);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					totlapoint = mySqlCommandHelper.GetInt("pointAccumulated");
					currentpoint = mySqlCommandHelper.GetInt("pointCurrent");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_requestUserPoint Error: {0}", ex.Message);
			}
		}

		/// <summary>Point type 2100 (0x834) = lobby/Shu HUD "%d Shu MP".</summary>
		private static int ReadShuMpPoint(int userNum)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdMP FROM UserShuInfo WHERE fdUserNum=@u LIMIT 1", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				object o = cmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					int mp = Convert.ToInt32(o);
					if (mp < 0)
					{
						return 0;
					}
					if (mp > 20)
					{
						return 20;
					}
					return mp;
				}
			}
			catch
			{
			}
			return 20;
		}

		private static bool HotKeySet(int UserNum, string HotKey)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_HotKeySet");
				mySqlCommandHelper.AddParamInt("UserNum", UserNum);
				mySqlCommandHelper.AddParamVarString("HotKey", HotKey);
				mySqlCommandHelper.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_HotKeySet Error: {0}", ex.Message);
			}
			return true;
		}

		private static void HotKeyGet(int UserNum, out string hkstr)
		{
			hkstr = string.Empty;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_HotKeyGet");
				mySqlCommandHelper.AddParamInt("UserNum", UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					hkstr = mySqlCommandHelper.GetString("fdHotKey");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_HotKeyGet Error: {0}", ex.Message);
			}
		}

		private static bool OpenCardPack(Account User, int cardpacknum, short opentype, out List<CardPackResultInfo> cardpackinfos)
		{
			cardpackinfos = new List<CardPackResultInfo>();
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_cardPack_CardOpen"))
				{
					mySqlCommandHelper.AddParamInt("pUsernum", User.UserNum);
					mySqlCommandHelper.AddParamInt("pUserLuck", (int)User.Luck);
					mySqlCommandHelper.AddParamInt("pCardPackNum", cardpacknum);
					mySqlCommandHelper.AddParamInt("pOpenType", opentype);
					mySqlCommandHelper.Execute();
					if (mySqlCommandHelper.HasRows)
					{
						long tR = User.TR;
						while (mySqlCommandHelper.HasResult())
						{
							tR = mySqlCommandHelper.GetLong("remainGameMoney");
							if (mySqlCommandHelper.GetInt("fdType") == 2)
							{
								CardPackResultInfo item = new CardPackResultInfo
								{
									RewardType = mySqlCommandHelper.GetInt("fdRewardType"),
									RewardItem = mySqlCommandHelper.GetInt("fdRewardItem"),
									RewardCount = mySqlCommandHelper.GetInt("fdRewardCount")
								};
								cardpackinfos.Add(item);
							}
						}
						User.TR = tR;
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_cardPack_CardOpen Error: {0}", ex.Message);
			}
			return false;
		}

		public static void Handle_AloneRunGameOver(ClientConnection Client, PacketReader reader, byte last)
		{
			int ms = 0;
			if (reader.Remaining >= 4)
			{
				ms = reader.ReadLEInt32();
			}
			Client.SendAsync(new AloneRunGameOver_ACK(ms, last));
		}

		public static void TBDyeInfo(string nickname, out List<UserItemDyeing> AvatarItemDyeing)
		{
			AvatarItemDyeing = new List<UserItemDyeing>();
			AvatarItemDyeing.AddRange(Enumerable.Repeat(new UserItemDyeing(), 24));
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemdyeing_tbinfo");
				mySqlCommandHelper.AddParamVarString("NickName", nickname);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					int index = mySqlCommandHelper.GetInt("fdNum") - 1;
					UserItemDyeing value = new UserItemDyeing
					{
						DyeingPart = mySqlCommandHelper.GetByte("fdPart"),
						Color1 = Utility.StringToByteArray(mySqlCommandHelper.GetString("fdColor1")),
						Color2 = Utility.StringToByteArray(mySqlCommandHelper.GetString("fdColor2")),
						Color3 = Utility.StringToByteArray(mySqlCommandHelper.GetString("fdColor3"))
					};
					AvatarItemDyeing[index] = value;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_itemdyeing_tbinfo Error: {0}", ex.Message);
			}
		}

		private static void getExp(Account User, short levelKind)
		{
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getExp"))
				{
					mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
					mySqlCommandHelper.AddParamShort("levelKind", levelKind);
					mySqlCommandHelper.ExecuteSingle();
					if (mySqlCommandHelper.HasResult() && levelKind == 1)
					{
						User.Exp = mySqlCommandHelper.GetLong("exp");
					}
				}
				User.EXPNeedUpdateFromDB = false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_getExp Error: {0}", ex.Message);
			}
		}
	}
}
