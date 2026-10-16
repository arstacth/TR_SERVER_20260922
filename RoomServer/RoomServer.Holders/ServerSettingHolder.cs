using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using RoomServer.Structuring;
using Serilog;

namespace RoomServer.Holders
{
	public static class ServerSettingHolder
	{
		public static List<SettingInfo> ServerSettingList { get; } = new List<SettingInfo>();


		public static ServerSetting ServerSettings { get; private set; }

		public static HashSet<string> HashList { get; } = new HashSet<string>();


		public static short ServerVersion { get; } = 1778;


		public static void LoadServerSettingInfo()
		{
			ServerSettingList.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM tblserversettinginfo", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					SettingInfo item = new SettingInfo
					{
						Key = mySqlDataReader["fdKey"].ToString().Replace(" ", ""),
						Value = mySqlDataReader["fdValue"].ToString(),
						OnlyServerSetting = Convert.ToBoolean(mySqlDataReader["fdOnlyServerSetting"])
					};
					ServerSettingList.Add(item);
				}
			}
			LoadDBInfo();
			Log.Information("Load ServerSetting Count: {0}", ServerSettingList.Count());
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

		private static int ParseSettingPercent(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return 0;
			}
			if (!float.TryParse(value.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f))
			{
				return 0;
			}
			if (f > 0f && f <= 1f)
			{
				return (int)Math.Round(f * 100f);
			}
			return (int)Math.Round(f);
		}

		private static void LoadDBInfo()
		{
			ServerSettings = null;
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
				case "BonusStageSelectRate":
					ServerSettings.BonusStageSelectRate = ParseSettingPercent(serverSetting.Value);
					break;
				case "BonusStage_Reward_Exp":
					ServerSettings.BonusStage_Reward_Exp = Convert.ToInt32(serverSetting.Value);
					break;
				case "BonusStage_Reward_TR":
					ServerSettings.BonusStage_Reward_TR = Convert.ToInt32(serverSetting.Value);
					break;
				case "UseBonusStageSystem":
					ServerSettings.UseBonusStageSystem = ParseSettingBool(serverSetting.Value);
					break;
				case "DEV_FESTIVAL_MinUserNum":
					ServerSettings.DEV_FESTIVAL_MinUserNum = Convert.ToByte(serverSetting.Value);
					break;
				case "DEV_FESTIVAL_MaxUserNum":
					ServerSettings.DEV_FESTIVAL_MaxUserNum = Convert.ToByte(serverSetting.Value);
					break;
				case "useThankOfferingSystem":
					ServerSettings.useThankOfferingSystem = ParseSettingBool(serverSetting.Value);
					break;
				case "ThankOfferingSchedule_CurNum":
					ServerSettings.ThankOfferingSchedule_CurNum = Convert.ToInt32(serverSetting.Value);
					break;
				case "OfficialCompetition_ArenaMode_MinUserNum":
					ServerSettings.OfficialCompetition_ArenaMode_MinUserNum = Convert.ToByte(serverSetting.Value);
					break;
				case "competitionRelayMatchMaxUserNum":
					ServerSettings.competitionRelayMatchMaxUserNum = Convert.ToByte(serverSetting.Value);
					break;
				case "competitionRelayMatchMinUserNum":
					ServerSettings.competitionRelayMatchMinUserNum = Convert.ToByte(serverSetting.Value);
					break;
				}
				}
				catch (Exception ex)
				{
					Log.Error("ServerSetting '{0}' value '{1}': {2}", serverSetting.Key, serverSetting.Value, ex.Message);
				}
			}
		}
	}
}
