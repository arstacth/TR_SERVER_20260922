using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class ServerSettingHolder
	{
		public static List<SettingInfo> ServerSettingList { get; } = new List<SettingInfo>();

		public static ServerSetting ServerSettings { get; private set; }

		public static HashSet<string> HashList { get; set; } = new HashSet<string>();

		public static void LoadServerSettingInfo()
		{
			ReadSettingsFromDb();
			UpsertRuntimeSetting("RelayServerIP", Conf.GetRelayAdvertiseIP());
			UpsertRuntimeSetting("RelayServerPort", Conf.RelayPort.ToString());
			UpsertRuntimeSetting("EnableAgentHashCheck", "false");
			UpsertRuntimeSetting("useAgentHashCheck", "false");
			UpsertRuntimeSetting("useEasyAntiCheat", "false");
			// Client-visible. TRUE at LOGIN_OK preloads HackAlarmPopup (1024_abusereport).
			UpsertRuntimeSetting("useHackingScreenShot", "false");
			BuildClientSettingPacket();
			LoadDBInfo();
			RebuildItemCollectionRank();
			Log.Information("Load ServerSetting Count: {0}", ServerSettingList.Count);
		}

		private static void ReadSettingsFromDb()
		{
			ServerSettingList.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("SELECT fdKey, fdValue, fdOnlyServerSetting FROM tblserversettinginfo", mySqlConnection);
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				ServerSettingList.Add(new SettingInfo
				{
					Key = mySqlDataReader["fdKey"].ToString().Replace(" ", ""),
					Value = mySqlDataReader["fdValue"] == DBNull.Value ? string.Empty : mySqlDataReader["fdValue"].ToString(),
					OnlyServerSetting = Convert.ToBoolean(mySqlDataReader["fdOnlyServerSetting"])
				});
			}
		}

		private static bool InsertSetting(string key, string value, bool onlyServer)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			if (key.Length > 80)
			{
				Log.Warning("Skip setting insert, key longer than fdKey(80): {0}", key);
				return false;
			}
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"INSERT INTO tblserversettinginfo (fdKey, fdValue, fdOnlyServerSetting, fdDesc) "
					+ "SELECT @k, @v, @only, 'auto' FROM DUAL "
					+ "WHERE NOT EXISTS (SELECT 1 FROM tblserversettinginfo WHERE fdKey=@k)",
					conn);
				cmd.Parameters.AddWithValue("@k", key);
				cmd.Parameters.AddWithValue("@v", value ?? string.Empty);
				cmd.Parameters.AddWithValue("@only", onlyServer ? 1 : 0);
				return cmd.ExecuteNonQuery() > 0;
			}
			catch (Exception ex)
			{
				Log.Warning("Insert setting {0}: {1}", key, ex.Message);
				return false;
			}
		}

		private static void UpsertRuntimeSetting(string key, string value)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand update = new MySqlCommand(
					"UPDATE tblserversettinginfo SET fdValue=@v WHERE fdKey=@k",
					conn);
				update.Parameters.AddWithValue("@k", key);
				update.Parameters.AddWithValue("@v", value ?? string.Empty);
				if (update.ExecuteNonQuery() > 0)
				{
					SettingInfo existing = ServerSettingList.FirstOrDefault((SettingInfo s) => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
					if (existing != null)
					{
						existing.Value = value;
					}
					return;
				}
				if (InsertSetting(key, value, onlyServer: false))
				{
					ServerSettingList.Add(new SettingInfo
					{
						Key = key,
						Value = value,
						OnlyServerSetting = false
					});
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Upsert setting {0}: {1}", key, ex.Message);
			}
		}

		private static void BuildClientSettingPacket()
		{
			List<SettingInfo> wireSettings = ServerSettingList.Where((SettingInfo w) => !w.OnlyServerSetting).ToList();
			PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
			packetWriter.WriteOP(Opcodes.eServer_GET_SERVERSETTING_INFO_ACK);
			packetWriter.Write(wireSettings.Count);
			foreach (SettingInfo item2 in wireSettings)
			{
				packetWriter.WriteBIG5Fixed_shortSize(item2.Key ?? string.Empty);
				packetWriter.WriteBIG5Fixed_shortSize(item2.Value ?? string.Empty);
			}
			DBInit.GameServerSetting = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			Log.Information("Client settings from DB count={0} bytes={1}", wireSettings.Count, DBInit.GameServerSetting.Length);
		}

		public static IEnumerable<SettingInfo> LobbyWireSettings()
		{
			return ServerSettingList.Where((SettingInfo w) => !w.OnlyServerSetting);
		}

		public static void RebuildItemCollectionRank()
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using (MySqlCommand updateOption = new MySqlCommand("UPDATE UserInfo SET fdOption = IFNULL(fdOption, 0) | 1", mySqlConnection))
				{
					updateOption.ExecuteNonQuery();
				}
				using (MySqlCommand truncate = new MySqlCommand("TRUNCATE TABLE GameDataItemCollectionRank", mySqlConnection))
				{
					truncate.ExecuteNonQuery();
				}
				using (MySqlCommand insert = new MySqlCommand(
					"INSERT INTO GameDataItemCollectionRank (fdRank, fdUserNum, fdNickName, fdPoint) "
					+ "SELECT ROW_NUMBER() OVER (ORDER BY t1.fdPoint DESC), t2.fdUserNum, t2.fdNickname, t1.fdPoint "
					+ "FROM UserItemCollection t1 INNER JOIN UserInfo t2 ON t1.fdUserNum = t2.fdUserNum "
					+ "WHERE t2.fdNickname IS NOT NULL AND t2.fdNickname <> ''",
					mySqlConnection))
				{
					int n = insert.ExecuteNonQuery();
					Log.Information("GameDataItemCollectionRank rows={0}", n);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("RebuildItemCollectionRank: {0}", ex.Message);
			}
		}

		public static void LoadHashList()
		{
			HashList.Clear();
			string[] array = File.ReadAllLines("hash.ini");
			foreach (string item in array)
			{
				HashList.Add(item);
			}
			Log.Information("Load Hash Count: {0}", HashList.Count);
		}

		private static bool ParseSettingBool(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string v = value.Trim();
			if (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase) || v.Equals("yes", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (v == "0" || v.Equals("false", StringComparison.OrdinalIgnoreCase) || v.Equals("no", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return Convert.ToBoolean(v);
		}

		private static void LoadDBInfo()
		{
			ServerSettings = new ServerSetting();
			foreach (SettingInfo serverSetting in ServerSettingList)
			{
				try
				{
					switch (serverSetting.Key)
					{
					case "charStatReset":
						ServerSettings.charStatReset = Convert.ToInt32(serverSetting.Value);
						break;
					case "charStatReset14":
						ServerSettings.charStatReset14 = Convert.ToInt32(serverSetting.Value);
						break;
					case "charStatSelectedReset":
						ServerSettings.charStatSelectedReset = Convert.ToInt32(serverSetting.Value);
						break;
					case "charStatUpgrade":
						ServerSettings.charStatUpgrade = Convert.ToInt32(serverSetting.Value);
						break;
					case "StatSystemStatPoint":
						ServerSettings.StatSystemStatPoint = Convert.ToInt32(serverSetting.Value);
						break;
					case "StatSystemMainStatMaxPoint":
						ServerSettings.StatSystemMainStatMaxPoint = Convert.ToInt32(serverSetting.Value);
						break;
					case "MultiplyTR":
						ServerSettings.MultiplyTR = Convert.ToSingle(serverSetting.Value);
						break;
					case "MultiplyEXP":
						ServerSettings.MultiplyEXP = Convert.ToSingle(serverSetting.Value);
						break;
					case "SurvivalMaxUserNum":
						ServerSettings.SurvivalMaxUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "SurvivalMinUserNum":
						ServerSettings.SurvivalMinUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "GateNoticeURL":
						ServerSettings.GateNoticeURL = serverSetting.Value;
						break;
					case "QuitConfirmDialogURL":
						ServerSettings.QuitConfirmDialogURL = serverSetting.Value;
						break;
					case "cashFillUpURL":
						ServerSettings.cashFillUpURL = serverSetting.Value;
						break;
					case "EveryDayEventURL":
						ServerSettings.EveryDayEventURL = serverSetting.Value;
						break;
					case "LoadingTimeOutMilliSeconds":
						ServerSettings.LoadingTimeOutMilliSeconds = Convert.ToInt32(serverSetting.Value);
						break;
					case "RABBIT_TURTLE_FATIGUE_DEC":
						ServerSettings.RABBIT_TURTLE_FATIGUE_DEC = Convert.ToSingle(serverSetting.Value);
						break;
					case "RABBIT_TURTLE_FATIGUE_INC":
						ServerSettings.RABBIT_TURTLE_FATIGUE_INC = Convert.ToSingle(serverSetting.Value);
						break;
					case "RABBIT_TURTLE_ITEM_FATIGUE_DEC":
						ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_DEC = Convert.ToSingle(serverSetting.Value);
						break;
					case "RABBIT_TURTLE_ITEM_FATIGUE_INC":
						ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_INC = Convert.ToSingle(serverSetting.Value);
						break;
					case "corunModeMinPlayerNum":
						ServerSettings.corunModeMinPlayerNum = Convert.ToByte(serverSetting.Value);
						break;
					case "corunModeDecreaseEnergyRatio":
						ServerSettings.corunModeDecreaseEnergyRatio = Convert.ToInt32(serverSetting.Value);
						break;
					case "useMapNumByRoomKindIDCheck":
						ServerSettings.useMapNumByRoomKindIDCheck = ParseSettingBool(serverSetting.Value);
						break;
					case "dungeonRaidMaxUserNum":
						ServerSettings.dungeonRaidMaxUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "dungeonRaidMinUserNum":
						ServerSettings.dungeonRaidMinUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "dungeonRaidGotPointConst":
						ServerSettings.dungeonRaidGotPointConst = Convert.ToSingle(serverSetting.Value);
						break;
					case "dungeonRaidBalanceConst":
						ServerSettings.dungeonRaidBalanceConst = Convert.ToInt32(serverSetting.Value);
						break;
					case "competitionMaxUserNum":
						ServerSettings.competitionMaxUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "competitionMinUserNum":
						ServerSettings.competitionMinUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "countrycode":
						ServerSettings.countrycode = serverSetting.Value;
						break;
					case "GiftItemLimitLevel":
						ServerSettings.GiftItemLimitLevel = Convert.ToInt32(serverSetting.Value);
						break;
					case "GMRoomMaxUserNum":
						ServerSettings.GMRoomMaxUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "competitionEventOn":
						ServerSettings.competitionEventOn = ParseSettingBool(serverSetting.Value);
						break;
					case "useCompetitionEvent":
						ServerSettings.useCompetitionEvent = Convert.ToInt32(serverSetting.Value);
						break;
					case "competitionEventMaxUserNum":
						ServerSettings.competitionEventMaxUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "competitionEventMinUserNum":
						ServerSettings.competitionEventMinUserNum = Convert.ToByte(serverSetting.Value);
						break;
					case "GuildMatchPartyMemberCount":
						ServerSettings.GuildMatchPartyMemberCount = Convert.ToInt32(serverSetting.Value);
						break;
					case "GuildMarkRegisterURL":
						ServerSettings.GuildMarkRegisterURL = serverSetting.Value;
						break;
					case "TowerEventTime":
						ServerSettings.TowerEventTime = Convert.ToInt32(serverSetting.Value);
						break;
					case "useThankOfferingSystem":
						ServerSettings.useThankOfferingSystem = ParseSettingBool(serverSetting.Value);
						break;
					case "ThankOfferingSchedule_CurNum":
						ServerSettings.ThankOfferingSchedule_CurNum = Convert.ToInt32(serverSetting.Value);
						break;
					case "useEasyAntiCheat":
						ServerSettings.useEasyAntiCheat = ParseSettingBool(serverSetting.Value);
						break;
					case "useXignCode":
						ServerSettings.useXignCode = ParseSettingBool(serverSetting.Value);
						break;
					case "competitionEventUsingRoomKind":
						ServerSettings.competitionEventUsingRoomKind = serverSetting.Value;
						break;
					}
				}
				catch (Exception ex)
				{
					Log.Error("ServerSetting '{0}' value '{1}': {2}", serverSetting.Key, serverSetting.Value, ex.Message);
				}
			}
			ServerSettings.useEasyAntiCheat = false;
		}
	}
}
