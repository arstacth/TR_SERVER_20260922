using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer;
using AgentServer.Database;
using AgentServer.Function;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class ItemHandle
	{
		public static void Handle_GetCurrentAvatarInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			bool bRequestNickName = reader.Remaining > 0 && reader.ReadBoolean();
			try
			{
				if (Conf.ProtocolDebug)
				{
					Log.Warning("GET_AVATAR_REQ user={0} nick={1}", Client.CurrentAccount.UserID, bRequestNickName);
				}
				getCurrentAvatarInfo(Client.CurrentAccount, bRequestNickName, last);
				if (Conf.ProtocolDebug)
				{
					Log.Warning("GET_AVATAR_REQ done user={0}", Client.CurrentAccount.UserID);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("GET_AVATAR_REQ exception user={0}: {1}", Client.CurrentAccount.UserID, ex);
				try
				{
					Client.CurrentAccount.Connection.SendAsync(new GET_AVATAR_FAIL_ACK(eServerResult.eServerResult_GET_AVATAR_FAILED_ACK, last));
				}
				catch (Exception sendEx)
				{
					Log.Warning("GET_AVATAR_REQ fail-ack also failed: {0}", sendEx.Message);
				}
			}
		}

		public static void Handle_GetAvatarItems(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			ushort charid = reader.ReadLEUInt16();
			int position = reader.ReadLEInt32();
			// Purge expired bag items before inventory sync (fishing rods etc.).
			getActiveFuncItem(currentAccount, -1, bExpiredCheck: true);
			getCharacterAvatarItem(currentAccount, charid, position, last);
		}

		public static void Handle_GetActiveFuncItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			bool bExpiredCheck = reader.ReadBoolean();
			bool bLevelLimit = reader.ReadBoolean();
			getActiveFuncItem(currentAccount, -1, bExpiredCheck, bLevelLimit);
		}

		public static void Handle_GetActiveFuncItem_Position(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int position = reader.ReadLEInt32();
			getActiveFuncItem(currentAccount, position);
		}

		public static void Handle_GetActiveFuncItem_List(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			string text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadLEInt32();
				text += $"{num2},";
			}
			getActiveFuncItemOne(currentAccount, text);
		}

		public static void Handle_GetAvatarItemOne(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			string itemnums = $"{num},";
			if (getAvatarItemOne(currentAccount, itemnums, out var aItemInfo))
			{
				if (aItemInfo.m_count <= 0 && ShopItemTable.getItemDataFromItemDescNum(aItemInfo.m_iItemDescNum, out var itemData) && itemData.m_iType != 1)
				{
					currentAccount.activeItem.deleteItem(aItemInfo.m_iItemDescNum);
				}
				Client.SendAsync(new GET_AVATAR_ITEM_ONE_ACK(aItemInfo, last));
			}
		}

		public static void Handle_GetAvatarItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			string text = string.Empty;
			List<int> list = new List<int>();
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadLEInt32();
				list.Add(num2);
				text += $"{num2},";
			}
			getAvatarItemAll(currentAccount, list, text, last);
		}

		public static bool checkInvalidItemDescNum(Account User, AvatarInfo userAvatarInfo, ref AvatarInfo originAvatarInfo, ref AvatarInfo transAvatarInfo, Dictionary<cpk_type, cpk_type> transKindNumMap, ref bool bWrongData)
		{
			try
			{
				if ((int)originAvatarInfo.m_pet != 0 && (int)originAvatarInfo.m_pet != 65535
					&& ShopItemTable.getRealItemDataFromCPK(0, (ushort)10, originAvatarInfo.m_pet, out var itemData))
				{
					cpk_type cpk_type = 0;
					if (itemData.m_mapAttr.ContainsKey(147))
					{
						cpk_type = (int)itemData.m_mapAttr[147];
					}
					if ((int)cpk_type != 0 && (int)originAvatarInfo.m_character != (int)cpk_type)
					{
						bWrongData = true;
					}
				}
				// Char IDs 28+ are valid on R186259 (older servers only allowed 1-27 and 201-222).
				int settingCharacter = (int)transAvatarInfo.m_character;
				if (settingCharacter <= 0)
				{
					return true;
				}
				if (settingCharacter > 0)
				{
					cpk_type cpk_type2 = transAvatarInfo.m_character;
					CItemTransformInfo itemTransformInfo = new CItemTransformInfo();
					foreach (KeyValuePair<cpk_type, cpk_type> item in transKindNumMap)
					{
						int worn = transAvatarInfo.GetWear((int)item.Key);
						if (worn == 0 || worn == 65535)
						{
							continue;
						}
						if (!TransformItemManager.getItemTransformInfo((cpk_type2, item.Key, item.Value), ref itemTransformInfo))
						{
							if (Conf.ProtocolDebug)
							{
								Log.Warning("checkInvalid skip missing transform user={0} c={1} slot={2} kind={3}",
									User.UserID, (int)cpk_type2, (int)item.Key, (int)item.Value);
							}
							continue;
						}
						cpk_type cpk_type3 = NetCommonFunc.getAvatarPartsByItemPosition(item.Key);
						if (NetCommonFunc.isWearItemPosition(cpk_type3))
						{
							originAvatarInfo.setItemPart(cpk_type3, itemTransformInfo.m_iOriginKind);
						}
						int itemDescNumFromCPK = ShopItemTable.getItemDescNumFromCPK(cpk_type2, item.Key, itemTransformInfo.m_iOriginKind);
						if (User.userItemAttr.getItemAttr(itemDescNumFromCPK, out CItemAttr rAttr))
						{
							float num = rAttr.m_attr[136];
							if ((int)item.Value != (short)num)
							{
								if (Conf.ProtocolDebug)
								{
									Log.Warning("checkInvalid transform attr mismatch user={0} desc={1} slot={2}",
										User.UserID, itemDescNumFromCPK, (int)item.Key);
								}
							}
						}
						else
						{
							if (Conf.ProtocolDebug)
							{
								Log.Warning("checkInvalid skip missing attr user={0} desc={1} slot={2}",
									User.UserID, itemDescNumFromCPK, (int)item.Key);
							}
						}
					}
					User.checkAvatar(ref transAvatarInfo, ref originAvatarInfo);
					if (((2123 <= (int)userAvatarInfo.m_gTopBody && 2138 >= (int)userAvatarInfo.m_gTopBody) || (2123 <= (int)originAvatarInfo.m_gTopBody && 2138 >= (int)originAvatarInfo.m_gTopBody) || 20028 == (int)transAvatarInfo.m_gTopBody) && ((int)userAvatarInfo.m_head != 0 || (int)userAvatarInfo.m_downBody != 0 || (int)userAvatarInfo.m_foot != 0 || (int)originAvatarInfo.m_head != 0 || (int)originAvatarInfo.m_downBody != 0 || (int)originAvatarInfo.m_foot != 0))
					{
						bWrongData = true;
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("checkInvalidItemDescNum: {0}", ex.ToString());
			}
			return false;
		}

		public static void getCurrentAvatarInfo(Account User, bool bRequestNickName, byte last)
		{
			bool flag = false;
			AdvancedAvatarInfo advancedAvatarInfo = new AdvancedAvatarInfo();
			try
			{
				flag = LoadCurrentAvatarFromTables(User.UserNum, ref advancedAvatarInfo);
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("GET_AVATAR load Error: {0}", ex.ToString());
			}
			if (Conf.ProtocolDebug)
			{
				Log.Warning("GET_AVATAR usp ok={0} character={1} top={2} cosTop={3} mode={4} user={5}",
					flag,
					(int)advancedAvatarInfo.getRealCharacter,
					(int)advancedAvatarInfo.m_realAvatarInfo.m_topBody,
					(int)advancedAvatarInfo.m_costumeAvatarInfo.m_topBody,
					advancedAvatarInfo.m_bIsUseCostume,
					User.UserID);
			}
			// Client GET_AVATAR_ACK: result 0 = OK, 0x3C = special, 0x3D = openStartCharacterUI.
			// Any other fail (incl. 62 GET_AVATAR_FAILED) only logs and continues — skips char select.
			// New register has no tblAvatarCharacterSetting row → load returns false; must send 0x3D.
			if (!flag || (int)advancedAvatarInfo.getRealCharacter == 0)
			{
				Log.Warning("GET_AVATAR no start character user={0} loadOk={1} char={2}",
					User.UserID, flag, (int)advancedAvatarInfo.getRealCharacter);
				User.Connection.SendAsync(new GET_AVATAR_FAIL_ACK((eServerResult)0x3D, last));
				return;
			}
			try
			{
				if (User.Attribute == 1)
				{
					ushort num = advancedAvatarInfo.m_realAvatarInfo.m_topBody;
					advancedAvatarInfo.clear();
					advancedAvatarInfo.setCharacter(101);
					advancedAvatarInfo.m_realAvatarInfo.m_topBody = num;
				}
				User.setAvatarInfoAndSendTCP(advancedAvatarInfo, bRequestNickName);
			}
			catch (Exception ex2)
			{
				Log.Warning("GET_AVATAR send path failed user={0}: {1}", User.UserID, ex2);
				try
				{
					User.setAvatarInfoAndSendTCP(advancedAvatarInfo, bRequestNickName);
				}
				catch (Exception ex3)
				{
					Log.Warning("GET_AVATAR fallback send failed user={0}: {1}", User.UserID, ex3);
					User.Connection.SendAsync(new GET_AVATAR_FAIL_ACK(eServerResult.eServerResult_GET_AVATAR_FAILED_ACK, last));
				}
			}
		}

		/// <summary>
		/// Load avatar from tables only. usp_getCurrentAvatarInfo calls usp_checkMyAvatarSetting
		/// which zeros starter CPK kinds the user does not own as tblAvatarUser rows.
		/// </summary>
		private static bool LoadCurrentAvatarFromTables(int userNum, ref AdvancedAvatarInfo av)
		{
			using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
			conn.Open();
			int settingNum = 0;
			using (MySqlCommand q = new MySqlCommand(
				"SELECT IFNULL(fdAvatarCharacterSettingNum,0) FROM UserInfoGame WHERE fdUserNum=@u", conn))
			{
				q.Parameters.AddWithValue("@u", userNum);
				object o = q.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					settingNum = Convert.ToInt32(o);
				}
			}
			using (MySqlCommand q = new MySqlCommand(@"
SELECT t1.fdCharacter AS `character`,
  IFNULL(t1.fdHead,0) AS head, IFNULL(t1.fdTopBody,0) AS topBody, IFNULL(t1.fdDownBody,0) AS downBody, IFNULL(t1.fdFoot,0) AS foot,
  IFNULL(t1.fdACHead,0) AS acHead, IFNULL(t1.fdACFace,0) AS acFace, IFNULL(t1.fdACHand,0) AS acHand,
  IFNULL(t1.fdACBack,0) AS acBack, IFNULL(t1.fdACNeck,0) AS acNeck, IFNULL(t1.fdPet,0) AS pet,
  IFNULL(t1.fdExpansion,0) AS expansion, IFNULL(t1.fdACWrist,0) AS acWrist, IFNULL(t1.fdACBooster,0) AS acBooster, IFNULL(t1.fdPart,0) AS acTail,
  IFNULL(t2.fdCharacter, t1.fdCharacter) AS cos_character,
  IFNULL(t2.fdHead,0) AS cos_head, IFNULL(t2.fdTopBody,0) AS cos_topBody, IFNULL(t2.fdDownBody,0) AS cos_downBody, IFNULL(t2.fdFoot,0) AS cos_foot,
  IFNULL(t2.fdACHead,0) AS cos_acHead, IFNULL(t2.fdACFace,0) AS cos_acFace, IFNULL(t2.fdACHand,0) AS cos_acHand,
  IFNULL(t2.fdACBack,0) AS cos_acBack, IFNULL(t2.fdACNeck,0) AS cos_acNeck, IFNULL(t2.fdPet,0) AS cos_pet,
  IFNULL(t2.fdExpansion,0) AS cos_expansion, IFNULL(t2.fdACWrist,0) AS cos_acWrist, IFNULL(t2.fdACBooster,0) AS cos_acBooster, IFNULL(t2.fdPart,0) AS cos_acTail,
  IFNULL(t3.fdCostumeMode,0) AS costumeMode
FROM tblAvatarCharacterSetting t1
LEFT JOIN UserCostumeCharacterSetting t2 ON t1.fdUserNum=t2.fdUserNum AND t1.fdCharacter=t2.fdCharacter
LEFT JOIN UserCostumeModeSetting t3 ON t1.fdUserNum=t3.fdUserNum
WHERE t1.fdUserNum=@u AND (@s=0 OR t1.fdItemCharacterSettingNum=@s)
ORDER BY t1.fdItemCharacterSettingNum
LIMIT 1", conn))
			{
				q.Parameters.AddWithValue("@u", userNum);
				q.Parameters.AddWithValue("@s", settingNum);
				using MySqlDataReader r = q.ExecuteReader();
				if (!r.Read() || r.IsDBNull(r.GetOrdinal("character")))
				{
					return false;
				}
				ReadAvatarRow(r, ref av.m_realAvatarInfo, costume: false);
				ReadAvatarRow(r, ref av.m_costumeAvatarInfo, costume: true);
				av.m_bIsUseCostume = Convert.ToInt32(r["costumeMode"]) != 0;
			}
			// Do not auto-equip owned starter clothes onto empty slots — default is no suit.
			if ((int)av.m_costumeAvatarInfo.m_character == 0 && (int)av.m_realAvatarInfo.m_character != 0)
			{
				av.m_costumeAvatarInfo.m_character = av.m_realAvatarInfo.m_character;
			}
			return (int)av.getRealCharacter != 0;
		}

		private static void ReadAvatarRow(MySqlDataReader r, ref AvatarInfo info, bool costume)
		{
			string p = costume ? "cos_" : "";
			info.m_character = ToUShort(r, p + "character");
			info.m_head = ToUShort(r, p + "head");
			info.m_topBody = ToUShort(r, p + "topBody");
			info.m_downBody = ToUShort(r, p + "downBody");
			info.m_foot = ToUShort(r, p + "foot");
			info.m_acHead = ToUShort(r, p + "acHead");
			info.m_acFace = ToUShort(r, p + "acFace");
			info.m_acHand = ToUShort(r, p + "acHand");
			info.m_acBack = ToUShort(r, p + "acBack");
			info.m_acNeck = ToUShort(r, p + "acNeck");
			info.m_pet = ToUShort(r, p + "pet");
			info.m_expansion = ToUShort(r, p + "expansion");
			info.m_acWrist = ToUShort(r, p + "acWrist");
			info.m_acBooster = ToUShort(r, p + "acBooster");
			info.m_accTail = ToUShort(r, p + "acTail");
		}

		private static ushort ToUShort(MySqlDataReader r, string col)
		{
			int i = r.GetOrdinal(col);
			if (r.IsDBNull(i))
			{
				return 0;
			}
			return (ushort)Convert.ToInt32(r.GetValue(i));
		}

		private static bool FillOwnedClothes(MySqlConnection conn, int userNum, ref AvatarInfo info)
		{
			int ch = (int)info.m_character;
			if (ch <= 0)
			{
				return false;
			}
			bool empty = info.m_head == 0 && info.m_topBody == 0 && info.m_downBody == 0 && info.m_foot == 0;
			if (!empty)
			{
				return false;
			}
			bool filled = false;
			using MySqlCommand q = new MySqlCommand(@"
SELECT T2.fdPos, T2.fdKind FROM tblAvatarUser T1
JOIN EssenAvatarItemCPKRef T2 ON T1.fdItemDescNum = T2.fdItemNum
WHERE T1.fdUserNum=@u AND T2.fdPos BETWEEN 1 AND 14 AND (T2.fdChar=@c OR T2.fdChar=0)
ORDER BY CASE WHEN T2.fdChar=@c THEN 0 ELSE 1 END, T1.fdItemDescNum", conn);
			q.Parameters.AddWithValue("@u", userNum);
			q.Parameters.AddWithValue("@c", ch);
			using MySqlDataReader r = q.ExecuteReader();
			while (r.Read())
			{
				int pos = Convert.ToInt32(r["fdPos"]);
				ushort kind = (ushort)Convert.ToInt32(r["fdKind"]);
				if (pos < 1 || pos > 14 || kind == 0)
				{
					continue;
				}
				if (info.GetWear(pos) == 0)
				{
					info.SetWear(pos, kind);
					filled = true;
				}
			}
			return filled;
		}

		public static void getActiveFuncItem(Account User, int position, bool bExpiredCheck = false, bool bLevelLimit = false, bool bEquipment = false)
		{
			List<NetItemInfo> list = new List<NetItemInfo>();
			bool flag = false;
			try
			{
				// Full refresh must purge expired rows. Cleanup only ran when items were
				// already in memory — but the SELECT excludes expired, so they stayed in
				// inventory forever (e.g. expired fishing rods still shown/usable).
				bool doExpireCheck = bExpiredCheck || position < 0;
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getActiveFuncItem"))
				{
					mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
					mySqlCommandHelper.AddParamInt("position", position);
					mySqlCommandHelper.AddParamInt("expiredcheck", doExpireCheck ? 1 : 0);
					mySqlCommandHelper.Execute();
					long nowMs = Utility.CurrentTimeMilliseconds();
					while (mySqlCommandHelper.HasResult())
					{
						NetItemInfo netItemInfo = new NetItemInfo();
						netItemInfo.clear();
						netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
						netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
						netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
						netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
						netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
						if (mySqlCommandHelper.IsDBNull("expireTime"))
						{
							netItemInfo.m_bHasExpireTime = false;
							netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
						}
						else
						{
							netItemInfo.m_bHasExpireTime = true;
							netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
							netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
						}
						netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
						// Never force-equip expired fishing gear (was always m_bUsing=true).
						bool fishingPos = netItemInfo.m_position == 185 || netItemInfo.m_position == 186;
						bool expired = netItemInfo.m_bHasExpireTime && netItemInfo.m_expireTime > 0L && netItemInfo.m_expireTime + 60000 <= nowMs;
						if (fishingPos && !expired)
						{
							netItemInfo.m_bUsing = true;
						}
						else if (expired)
						{
							netItemInfo.m_bUsing = false;
						}
						// 251-253=LevelUP reward boxes: client auto-opens empty
						// LuckyBagItemCollectionLevelUpReward.gui when these are wired.
						// Keep 250 (LuckyBag) — deleting/hiding them made bags unopenable.
						if (netItemInfo.m_position >= 251 && netItemInfo.m_position <= 253)
						{
							continue;
						}
						list.Add(netItemInfo);
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getActiveFuncItem Error: {0}", ex.ToString());
			}
			if (flag)
			{
				if (position < 0)
				{
					User.onRecvActiveFuncItem(flag, list, bEquipment);
				}
				else
				{
					User.onRecvActiveFuncItem(flag, list, position);
				}
				User.modifyActiveItemForRoom();
			}
		}

		private static void getActiveFuncItemOne(Account User, string itemnums)
		{
			Dictionary<int, NetItemInfo> dictionary = new Dictionary<int, NetItemInfo>();
			bool flag = false;
			try
			{
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getActiveFuncItemOne"))
				{
					mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
					mySqlCommandHelper.AddParamVarString("itemnums", itemnums);
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						NetItemInfo netItemInfo = new NetItemInfo();
						netItemInfo.clear();
						netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
						netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
						netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
						netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
						netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
						if (mySqlCommandHelper.IsDBNull("expireTime"))
						{
							netItemInfo.m_bHasExpireTime = false;
							netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
						}
						else
						{
							netItemInfo.m_bHasExpireTime = true;
							netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
							netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
						}
						netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
						long nowMs = Utility.CurrentTimeMilliseconds();
						bool fishingPos = netItemInfo.m_position == 185 || netItemInfo.m_position == 186;
						bool expired = netItemInfo.m_bHasExpireTime && netItemInfo.m_expireTime > 0L && netItemInfo.m_expireTime + 60000 <= nowMs;
						if (fishingPos && !expired)
						{
							netItemInfo.m_bUsing = true;
						}
						else if (expired)
						{
							netItemInfo.m_bUsing = false;
						}
						dictionary[netItemInfo.m_iItemDescNum] = netItemInfo;
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getActiveFuncItemOne Error: {0}", ex.ToString());
			}
			if (flag)
			{
				User.onRecvActiveFuncItem(flag, dictionary);
				User.modifyActiveItemForRoom();
			}
		}

		public static int ResolveGiftItemDescNum(int itemDescNum)
		{
			if (ShopItemTable.getItemDataFromItemDescNum(itemDescNum, out var virt) && virt.m_iType == 3)
			{
				if (virt.m_iSupplyItemDescNum != 0)
				{
					return virt.m_iSupplyItemDescNum;
				}
				int fromRealList = LookupRealItemDescNum(itemDescNum);
				if (fromRealList > 0)
				{
					return fromRealList;
				}
			}
			if (ShopItemTable.getRealItemDataFromItemDescNum(itemDescNum, out var realData) && realData.m_iItemDescNum != 0
				&& realData.m_iItemDescNum != itemDescNum)
			{
				return realData.m_iItemDescNum;
			}
			return itemDescNum;
		}

		/// <summary>
		/// Client sends PERMANENCE_ITEM_REQ in a tight loop while inventory is open.
		/// Never reply with ITEM_ONE / ACTIVE_FUNC — that Overpop'd 1503 and crashed the client.
		/// Thai has no known permanence ACK; ignore until a verified ACK layout exists.
		/// </summary>
		public static void Handle_PermanenceItem(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client;
			if (reader.Remaining >= 8)
			{
				reader.ReadLEInt32();
				reader.ReadLEInt32();
			}
			else if (reader.Remaining >= 4)
			{
				reader.ReadLEInt32();
			}
			_ = last;
		}

		public static int ResolveInventoryItemDescNum(int itemDescNum)
		{
			int resolved = ResolveGiftItemDescNum(itemDescNum);
			if (ShopItemTable.getItemDataFromItemDescNum(resolved, out var data) && data.m_iType == 3)
			{
				int realNum = LookupRealItemDescNum(resolved);
				if (realNum > 0)
				{
					return realNum;
				}
			}
			return resolved;
		}

		private static int LookupRealItemDescNum(int packageNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using (MySqlCommand mySqlCommand = new MySqlCommand(
						"SELECT fdRealItemDescNum FROM tblAvatarItemRealList WHERE fdItemDescNum=@p LIMIT 1",
						mySqlConnection))
					{
						mySqlCommand.Parameters.AddWithValue("@p", packageNum);
						object obj = mySqlCommand.ExecuteScalar();
						if (obj != null && obj != DBNull.Value)
						{
							return Convert.ToInt32(obj);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("LookupRealItemDescNum package={0}: {1}", packageNum, ex.Message);
			}
			return 0;
		}

		public static void SendAvatarItemOne(Account User, int itemDescNum, byte last)
		{
			int lookup = ResolveInventoryItemDescNum(itemDescNum);
			if (getAvatarItemOne(User, $"{lookup},", out var aItemInfo) && aItemInfo.m_count > 0)
			{
				User.activeItem.replaceItemInfoByItemNum(new Dictionary<int, NetItemInfo>
				{
					{ aItemInfo.m_iItemDescNum, aItemInfo }
				});
				User.Connection.SendAsync(new GET_AVATAR_ITEM_ONE_ACK(aItemInfo, last));
			}
		}

		public static void PrepareBoughtItems(List<ShopBuyItemInfo> list)
		{
			foreach (ShopBuyItemInfo item in list)
			{
				if (!item.BuySuccess)
				{
					continue;
				}
				int inv = ResolveInventoryItemDescNum(item.ItemNum);
				item.supplyItemDescNum = inv;
				if (ShopItemTable.getItemDataFromItemDescNum(inv, out var data))
				{
					item.ItemPosition = data.m_iPosition;
				}
			}
		}

		public static void NotifyBoughtItems(Account User, List<ShopBuyItemInfo> list, byte last)
		{
			HashSet<int> positions = new HashSet<int>();
			foreach (ShopBuyItemInfo item in list)
			{
				if (!item.BuySuccess)
				{
					continue;
				}
				int inv = item.supplyItemDescNum > 0 ? item.supplyItemDescNum : ResolveInventoryItemDescNum(item.ItemNum);
				if (item.ItemPosition > 0)
				{
					positions.Add(item.ItemPosition);
				}
				SendAvatarItemOne(User, inv, last);
			}
			foreach (int position in positions)
			{
				getActiveFuncItem(User, position);
			}
		}

		public static bool getAvatarItemOne(Account User, string itemnums, out NetItemInfo aItemInfo)
		{
			aItemInfo = new NetItemInfo();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getAvatarItem");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamVarString("itemnums", itemnums);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					aItemInfo.clear();
					aItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
					aItemInfo.m_character = mySqlCommandHelper.GetInt("character");
					aItemInfo.m_position = mySqlCommandHelper.GetInt("position");
					aItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
					aItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					aItemInfo.m_exp = mySqlCommandHelper.GetInt("exp");
					aItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
					aItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
					if (mySqlCommandHelper.IsDBNull("expireTime"))
					{
						aItemInfo.m_bHasExpireTime = false;
					}
					else
					{
						aItemInfo.m_bHasExpireTime = true;
						aItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getAvatarItem Error: {0}", ex.ToString());
				return flag;
			}
		}

		private static void getAvatarItemAll(Account User, List<int> itemnum, string itemnums, byte last)
		{
			Dictionary<int, NetItemInfo> dictionary = new Dictionary<int, NetItemInfo>();
			List<NetItemInfo> list = new List<NetItemInfo>();
			bool flag = false;
			try
			{
				if (itemnum.Count > 500)
				{
					using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getAvatarItem_all");
					mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						NetItemInfo netItemInfo = new NetItemInfo();
						netItemInfo.clear();
						netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
						if (itemnum.Contains(netItemInfo.m_iItemDescNum))
						{
							netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
							netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
							netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
							netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
							netItemInfo.m_exp = mySqlCommandHelper.GetInt("exp");
							netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
							netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
							if (mySqlCommandHelper.IsDBNull("expireTime"))
							{
								netItemInfo.m_bHasExpireTime = false;
							}
							else
							{
								netItemInfo.m_bHasExpireTime = true;
								netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
							}
							list.Add(netItemInfo);
							if (mySqlCommandHelper.GetIntConvert("isActiveItem") > 0)
							{
								dictionary[netItemInfo.m_iItemDescNum] = netItemInfo;
							}
						}
					}
					flag = true;
				}
				else
				{
					using MySqlCommandHelper mySqlCommandHelper2 = new MySqlCommandHelper("usp_getAvatarItem");
					mySqlCommandHelper2.AddParamInt("usernum", User.UserNum);
					mySqlCommandHelper2.AddParamVarString("itemnums", itemnums);
					mySqlCommandHelper2.Execute();
					while (mySqlCommandHelper2.HasResult())
					{
						NetItemInfo netItemInfo2 = new NetItemInfo();
						netItemInfo2.m_iItemDescNum = mySqlCommandHelper2.GetInt("itemdescnum");
						netItemInfo2.m_character = mySqlCommandHelper2.GetInt("character");
						netItemInfo2.m_position = mySqlCommandHelper2.GetInt("position");
						netItemInfo2.m_kind = mySqlCommandHelper2.GetInt("kind");
						netItemInfo2.m_count = mySqlCommandHelper2.GetInt("count");
						netItemInfo2.m_exp = mySqlCommandHelper2.GetInt("exp");
						netItemInfo2.m_bUsing = mySqlCommandHelper2.GetBoolean("using");
						netItemInfo2.m_tGot = mySqlCommandHelper2.GetDateTime("gotDateTime", 0L);
						if (mySqlCommandHelper2.IsDBNull("expireTime"))
						{
							netItemInfo2.m_bHasExpireTime = false;
						}
						else
						{
							netItemInfo2.m_bHasExpireTime = true;
							netItemInfo2.m_expireTime = mySqlCommandHelper2.GetDateTime("expiretime", 0L);
						}
						list.Add(netItemInfo2);
						if (mySqlCommandHelper2.GetIntConvert("isActiveItem") > 0)
						{
							dictionary[netItemInfo2.m_iItemDescNum] = netItemInfo2;
						}
					}
					flag = true;
				}
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getAvatarItem Error: {0}", ex.ToString());
			}
			if (!flag)
			{
				return;
			}
			User.activeItem.replaceItemInfoByItemNum(dictionary);
			List<List<NetItemInfo>> list2 = list.Split();
			byte b = 0;
			foreach (List<NetItemInfo> item in list2)
			{
				short startindex = (short)(1630 * b);
				b = (byte)(b + 1);
				byte remainpage = (byte)(list2.Count - b);
				User.Connection.SendAsync(new GET_AVATAR_ITEM_LIST_ACK(User, startindex, remainpage, item, last));
			}
		}

		private static void getCharacterAvatarItem(Account User, int charid, int position, byte last)
		{
			int requestChar = charid;
			int requestPos = position;
			Dictionary<int, List<NetItemInfo>> dictionary = new Dictionary<int, List<NetItemInfo>>();
			if (!User.isRecvAllofCharItem)
			{
				charid = -1;
				position = -1;
			}
			HashSet<int> owned = GetOwnedCharacterIds(User.UserNum);
			if (requestChar > 0)
			{
				owned.Add(requestChar);
			}
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getCharacterAvatarItem");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pcharacter", charid);
				mySqlCommandHelper.AddParamInt("position", position);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					NetItemInfo netItemInfo = new NetItemInfo();
					netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
					netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
					netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
					netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
					netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					netItemInfo.m_exp = mySqlCommandHelper.GetInt("exp");
					netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
					netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
					if (mySqlCommandHelper.IsDBNull("expireTime"))
					{
						netItemInfo.m_bHasExpireTime = false;
					}
					else
					{
						netItemInfo.m_bHasExpireTime = true;
						netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
					}
					if (dictionary.ContainsKey(netItemInfo.m_character))
					{
						dictionary[netItemInfo.m_character].Add(netItemInfo);
						continue;
					}
					dictionary.Add(netItemInfo.m_character, new List<NetItemInfo> { netItemInfo });
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getCharacterAvatarItem Error: {0}", ex.ToString());
			}
			if (!flag)
			{
				return;
			}
			if (!User.isRecvAllofCharItem)
			{
				User.isRecvAllofCharItem = true;
			}
			if (dictionary.Count == 0)
			{
				User.Connection.SendAsync(new GET_AVATAR_ITEMS_ACK(requestChar, requestPos, 0, 0, new List<NetItemInfo>(), last));
				return;
			}
			foreach (KeyValuePair<int, List<NetItemInfo>> item in dictionary)
			{
				if (owned.Count > 0 && item.Key != 0 && !owned.Contains(item.Key))
				{
					continue;
				}
				List<List<NetItemInfo>> list = item.Value.Split();
				byte b = 0;
				if (list.Count == 0)
				{
					User.Connection.SendAsync(new GET_AVATAR_ITEMS_ACK(item.Key, position, 0, 0, new List<NetItemInfo>(), last));
					continue;
				}
				foreach (List<NetItemInfo> item2 in list)
				{
					short startindex = (short)(1630 * b);
					b = (byte)(b + 1);
					byte remainpage = (byte)(list.Count - b);
					User.Connection.SendAsync(new GET_AVATAR_ITEMS_ACK(item.Key, position, remainpage, startindex, item2, last));
				}
			}
		}

		internal static HashSet<int> GetOwnedCharacterIds(int userNum)
		{
			HashSet<int> owned = new HashSet<int>();
			if (userNum <= 0)
			{
				return owned;
			}
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					@"SELECT DISTINCT IFNULL(NULLIF(t2.fdChar,0), d.fdCharacter)
FROM tblAvatarUserCharacter t1
LEFT JOIN EssenAvatarItemCPKRef t2 ON t1.fdItemDescNum = t2.fdItemNum
LEFT JOIN tblAvatarItemDesc d ON t1.fdItemDescNum = d.fdItemNum
WHERE t1.fdUserNum=@u
  AND ((t2.fdPos=0 AND t2.fdChar<>0) OR (d.fdPosition=0 AND IFNULL(d.fdCharacter,0)<>0))",
					conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				using MySqlDataReader r = cmd.ExecuteReader();
				while (r.Read())
				{
					if (r.IsDBNull(0))
					{
						continue;
					}
					owned.Add(Convert.ToInt32(r[0]));
				}
			}
			catch (Exception ex)
			{
				Log.Warning("GetOwnedCharacterIds: {0}", ex.Message);
			}
			return owned;
		}
	}
}
