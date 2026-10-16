using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class CompetitionEventHandle
	{
		public enum eCompetitionEventResult
		{
			eCompetitionEventResult_OK,
			eCompetitionEventResult_DBERROR,
			eCompetitionEventResult_ALREADY_RECV_REWARD,
			eCompetitionEventResult_NOT_WINNER_PARTY,
			eCompetitionEventResult_ALREADY_RECV_POINT_REWARD,
			eCompetitionEventResult_NOT_ENOUGH_POINT,
			eCompetitionEventResult_NOT_NEXT_POINT_REWARD,
			eCompetitionEventResult_REWARD_LIMIT_TIME_OVER,
			eCompetitionEventResult_JOIN_PARTY_FAILED_INVALID_GENDER,
			eCompetitionEventResult_CANT_JOIN_SELECT_PARTY,
			eCompetitionEventResult_NOT_FOUND_TERRITORY_INFO,
			eCompetitionEventResult_NO_SEASONPASS_FOR_REWARD
		}

		private static readonly object eventJoinLock = new object();

		public static void Handle_GetPartyPointInfo(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			competitionEvent_partyInfo(out var CompetitionPartyInfo);
			Client.SendAsync(new CompetitionEvent_PartyInfo(CompetitionPartyInfo, last));
		}

		public static void Handle_PartyJoin(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (!ServerSettingHolder.ServerSettings.competitionEventOn)
			{
				return;
			}
			lock (eventJoinLock)
			{
				List<ExchangeItemInfo> exinfo;
				int joinPartyType;
				int SubPartyType;
				eCompetitionEventResult result;
				bool alreadyJoined = TryLoadExistingParty(currentAccount.UserNum, out joinPartyType, out SubPartyType);
				if (!alreadyJoined)
				{
					competitionEvent_partyJoin(currentAccount.UserNum, out exinfo, out joinPartyType, out SubPartyType, out result, out alreadyJoined);
				}
				else
				{
					exinfo = new List<ExchangeItemInfo>();
					result = eCompetitionEventResult.eCompetitionEventResult_OK;
				}
				if (alreadyJoined)
				{
					// Already on a team: never resend JoinOK (empty rewards still open first-reward UI).
					Log.Information("PartyJoin already joined user={0} party={1} sub={2}",
						currentAccount.UserNum, joinPartyType, SubPartyType);
					currentAccount.PartyType = (short)joinPartyType;
					currentAccount.SubPartyType = SubPartyType;
					competitionEvent_partyUserInfo(currentAccount.UserNum, out SubPartyType, out var existingInfo);
					if (SubPartyType > 0)
					{
						currentAccount.SubPartyType = SubPartyType;
					}
					currentAccount.CompetitionEventJoinAckSent = true;
					Client.SendAsync(new CompetitionEvent_PartyUserInfo(joinPartyType, SubPartyType, existingInfo, last));
					Client.SendAsync(new CompetitionEvent_RoomKindPlayerNum(last));
					Client.SendAsync(new SEASON_CHANNEL_SCHEDULE_ACK(last));
					return;
				}
				Client.SendAsync(new CompetitionEvent_PartyJoinOK(result, joinPartyType, SubPartyType, exinfo, last));
				if (result == eCompetitionEventResult.eCompetitionEventResult_OK)
				{
					currentAccount.CompetitionEventJoinAckSent = true;
					currentAccount.PartyType = (short)joinPartyType;
					currentAccount.SubPartyType = SubPartyType;
					competitionEvent_partyUserInfo(currentAccount.UserNum, out SubPartyType, out var partyUserInfo);
					if (SubPartyType > 0)
					{
						currentAccount.SubPartyType = SubPartyType;
					}
					Client.SendAsync(new CompetitionEvent_PartyUserInfo(joinPartyType, SubPartyType, partyUserInfo, last));
					Client.SendAsync(new CompetitionEvent_RoomKindPlayerNum(last));
					Client.SendAsync(new SEASON_CHANNEL_SCHEDULE_ACK(last));
				}
			}
		}

		public static void Handle_PartyUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			competitionEvent_partyUserInfo(currentAccount.UserNum, out var SubPartyType, out var CompetitionPartyInfo);
			int partyType = currentAccount.PartyType;
			if (partyType <= 0 && TryLoadExistingParty(currentAccount.UserNum, out int loadedParty, out int loadedSub))
			{
				partyType = loadedParty;
				SubPartyType = loadedSub;
				currentAccount.PartyType = (short)loadedParty;
				currentAccount.SubPartyType = loadedSub;
			}
			else if (SubPartyType > 0)
			{
				currentAccount.SubPartyType = SubPartyType;
			}
			else if (currentAccount.SubPartyType > 0)
			{
				SubPartyType = currentAccount.SubPartyType;
			}
			else if (partyType > 0)
			{
				SubPartyType = EnsureDefaultSubParty(partyType);
				currentAccount.SubPartyType = SubPartyType;
			}
			if (partyType > 0)
			{
				// Never emit JoinOK here — empty JoinOK still pops first-join reward UI.
				// PartyType comes from login LoadParty / first Handle_PartyJoin only.
				currentAccount.CompetitionEventJoinAckSent = true;
				Client.SendAsync(new CompetitionEvent_RoomKindPlayerNum(last));
				Client.SendAsync(new SEASON_CHANNEL_SCHEDULE_ACK(last));
			}
			Client.SendAsync(new CompetitionEvent_PartyUserInfo(partyType, SubPartyType, CompetitionPartyInfo, last));
		}

		/// <summary>Load joined AnimalVillage party onto the account at login so intro can skip.</summary>
		public static void LoadPartyOntoAccount(Account account)
		{
			if (account == null || account.UserNum <= 0)
			{
				return;
			}
			if (TryLoadExistingParty(account.UserNum, out int party, out int sub) && party > 0)
			{
				account.PartyType = (short)party;
				account.SubPartyType = sub > 0 ? sub : EnsureDefaultSubParty(party);
			}
		}

		/// <summary>
		/// Already on a team: PartyUserInfo + maps only. JoinOK with rewards is first-join
		/// only (Handle_PartyJoin). Empty JoinOK at login replayed the reward UI.
		/// </summary>
		public static void SendAlreadyJoinedAtLogin(ClientConnection Client, byte last)
		{
			Account account = Client?.CurrentAccount;
			if (account == null || account.PartyType <= 0)
			{
				return;
			}
			int sub = account.SubPartyType > 0 ? account.SubPartyType : EnsureDefaultSubParty(account.PartyType);
			account.SubPartyType = sub;
			account.CompetitionEventJoinAckSent = true;
			competitionEvent_partyUserInfo(account.UserNum, out _, out var info);
			Client.SendAsync(new CompetitionEvent_PartyUserInfo(account.PartyType, sub, info, last));
			Client.SendAsync(new CompetitionEvent_RoomKindPlayerNum(last));
			Client.SendAsync(new SEASON_CHANNEL_SCHEDULE_ACK(last));
		}

		public static void Handle_TodayGameInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new CompetitionEvent_TodayGameInfo(last));
		}

		public static void Handle_FronTier_SchduleInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new FronTier_SchduleInfo(last));
		}

		public static void Handle_SEASON_CHANNEL_SCHEDULE_REQ(ClientConnection Client, byte last)
		{
			Client.SendAsync(new SEASON_CHANNEL_SCHEDULE_ACK(last));
		}

		public static void Handle_PointReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short rewardLevel = reader.ReadLEInt16();
			bool flag = reader.ReadBoolean();
			if (flag && !currentAccount.activeItem.HasPosition(820))
			{
				Client.SendAsync(new CompetitionEvent_PointReward(eCompetitionEventResult.eCompetitionEventResult_NO_SEASONPASS_FOR_REWARD, rewardLevel, flag, null, last));
			}
			else if (ServerSettingHolder.ServerSettings.competitionEventOn)
			{
				competitionEvent_pointReward(currentAccount.UserNum, rewardLevel, flag, out var exinfo, out var result);
				Client.SendAsync(new CompetitionEvent_PointReward(result, rewardLevel, flag, exinfo, last));
			}
		}

		public static void Handle_GetRoomKindPlayerNum(ClientConnection Client, byte last)
		{
			Client.SendAsync(new CompetitionEvent_RoomKindPlayerNum(last));
		}

		public static void Handle_NewSeasonPassUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = reader;
			Account user = Client.CurrentAccount;
			List<NewSeasonPassRow> rows = LoadNewSeasonPassRows(user != null ? user.UserNum : 0);
			Client.SendAsync(new NewSeasonPass_UserInfoAck(rows, last));
		}

		public static void Handle_TeamPointGathering(ClientConnection Client, byte last)
		{
			Client.SendAsync(new CompetitionEvent_TeamPointAck(LoadTeamPoints(), last));
		}

		public static void Handle_PointGatheringMyInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = reader;
			Client.SendAsync(new PointGathering_MyInfoAck(last));
		}

		private static List<NewSeasonPassRow> LoadNewSeasonPassRows(int userNum)
		{
			List<NewSeasonPassRow> list = new List<NewSeasonPassRow>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(@"
SELECT e.fdKind AS SeasonPassKind,
       IFNULL(r.fdGroupNum, 0) AS `Group`,
       IFNULL(r.fdFreeSPRewardLvl, 0) AS FreeSPRewardLvl,
       IFNULL(r.fdPlusSPRewardLvl, 0) AS PlusSPRewardLvl,
       IFNULL(p.fdPoint, 0) AS Point,
       IFNULL(e.fdUsePeriod, 0) AS usePeriod,
       IFNULL(UNIX_TIMESTAMP(e.fdStartTime), 0) * 1000 AS startMs,
       IFNULL(UNIX_TIMESTAMP(e.fdEndTime), 0) * 1000 AS endMs,
       IFNULL(p.fdAccEnable, 1) AS AccEnable
FROM EssenNewSeasonPass e
LEFT JOIN UserNewSeasonPassPoint p
  ON p.fdUserNum = @u AND p.fdSeasonPassKind = e.fdKind
LEFT JOIN UserNewSeasonPassReward r
  ON r.fdUserNum = @u AND r.fdSeasonPassKind = e.fdKind AND r.fdGroupNum = 1
WHERE (e.fdUsePeriod = 1 OR p.fdPoint IS NOT NULL OR e.fdKind IN (12, 13, 14))
ORDER BY e.fdKind
", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					NewSeasonPassRow row = default(NewSeasonPassRow);
					row.Kind = Convert.ToInt32(reader["SeasonPassKind"]);
					row.Group = Convert.ToInt32(reader["Group"]);
					row.FreeLvl = Convert.ToByte(reader["FreeSPRewardLvl"]);
					row.PlusLvl = Convert.ToByte(reader["PlusSPRewardLvl"]);
					row.Point = Convert.ToInt32(reader["Point"]);
					row.UsePeriod = Convert.ToInt32(reader["usePeriod"]);
					row.StartMs = NormalizePassTime(Convert.ToInt64(reader["startMs"]));
					row.EndMs = NormalizePassTime(Convert.ToInt64(reader["endMs"]));
					// Expired/zero-length window (old Clamp→2000-01-01) keeps Animal Village intro looping.
					if (row.StartMs <= 0 || row.EndMs <= 0 || row.StartMs >= row.EndMs)
					{
						row.StartMs = NormalizePassTime(Utility.ConvertToTimestamp(new DateTime(2025, 1, 1)));
						row.EndMs = NormalizePassTime(Utility.ConvertToTimestamp(new DateTime(2037, 12, 31, 23, 59, 59)));
					}
					// Client summer2025_seasonpassdata NewSeasonPassGroup=1.
					if (row.Group <= 0)
					{
						row.Group = 1;
					}
					row.AccEnable = Convert.ToByte(reader["AccEnable"]);
					list.Add(row);
				}
			}
			catch (Exception ex)
			{
				Log.Error("LoadNewSeasonPassRows Error:{0}", ex.Message);
			}
			return list;
		}

		private static long NormalizePassTime(long ms)
		{
			// Seconds mistaken for ms clamp to year-2000 and break season-pass / intro.
			if (ms > 0 && ms < 10000000000L)
			{
				ms *= 1000L;
			}
			const long minOk = 946684800000L; // 2000-01-01
			const long maxOk = 4102444800000L; // 2100-01-01
			if (ms < minOk)
			{
				return 0L;
			}
			if (ms > maxOk)
			{
				return maxOk;
			}
			return ms;
		}

		private static List<KeyValuePair<int, int>> LoadTeamPoints()
		{
			List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				// Official dbgtrace logs TeamPointAck (1). 141 is a SubParty of 140, not a
				// second gathering row — sending both made Ack (2) vs official (1).
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdPartyType, fdPoint FROM GameDataCompetitionEvent_PartyPointGathering "
					+ "WHERE fdPartyType = 140 LIMIT 1",
					conn);
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					list.Add(new KeyValuePair<int, int>(
						Convert.ToInt32(reader["fdPartyType"]),
						Convert.ToInt32(reader["fdPoint"])));
				}
			}
			catch (Exception ex)
			{
				Log.Error("LoadTeamPoints Error:{0}", ex.Message);
			}
			if (list.Count == 0)
			{
				list.Add(new KeyValuePair<int, int>(140, 0));
			}
			return list;
		}

		private static void competitionEvent_partyJoin(int UserNum, out List<ExchangeItemInfo> exinfo, out int joinPartyType, out int SubPartyType, out eCompetitionEventResult result, out bool alreadyJoined)
		{
			exinfo = new List<ExchangeItemInfo>();
			joinPartyType = 0;
			SubPartyType = 0;
			alreadyJoined = false;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_competitionEvent_partyJoin";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("eventType", MySqlDbType.Int16).Value = ServerSettingHolder.ServerSettings.useCompetitionEvent;
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
				mySqlDataReader.NextResult();
				mySqlDataReader.Read();
				joinPartyType = mySqlDataReader.GetInt16("joinPartyType");
				SubPartyType = mySqlDataReader.GetInt16("SubPartyType");
				result = eCompetitionEventResult.eCompetitionEventResult_OK;
			}
			catch (MySqlException ex)
			{
				if (ex.Message != null && ex.Message.IndexOf("already join party", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					if (TryLoadExistingParty(UserNum, out joinPartyType, out SubPartyType))
					{
						alreadyJoined = true;
						result = eCompetitionEventResult.eCompetitionEventResult_OK;
						Log.Information("usp_competitionEvent_partyJoin already joined user={0} party={1}", UserNum, joinPartyType);
						return;
					}
				}
				result = eCompetitionEventResult.eCompetitionEventResult_DBERROR;
				Log.Error("usp_competitionEvent_partyJoin Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = eCompetitionEventResult.eCompetitionEventResult_DBERROR;
				Log.Error("usp_competitionEvent_partyJoin Error:{0}", ex2.Message);
			}
		}

		private static bool TryLoadExistingParty(int UserNum, out int joinPartyType, out int SubPartyType)
		{
			joinPartyType = 0;
			SubPartyType = 0;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdPartyType, IFNULL(fdSubPartyType,0) AS fdSubPartyType FROM UserCompetitionEventPartyInfo "
					+ "WHERE fdUserNum=@u "
					+ "ORDER BY fdJoinDateTime DESC LIMIT 1",
					conn);
				cmd.Parameters.AddWithValue("@u", UserNum);
				using MySqlDataReader reader = cmd.ExecuteReader();
				if (!reader.Read())
				{
					return false;
				}
				joinPartyType = Convert.ToInt32(reader["fdPartyType"]);
				SubPartyType = Convert.ToInt32(reader["fdSubPartyType"]);
				if (SubPartyType <= 0)
				{
					SubPartyType = EnsureDefaultSubParty(joinPartyType);
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("TryLoadExistingParty Error:{0}", ex.Message);
				return false;
			}
		}

		private static int EnsureDefaultSubParty(int mainPartyType)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdSubPartyType FROM EssenCompetitionEventSubParty WHERE fdMainPartyType=@p ORDER BY fdSubPartyType LIMIT 1",
					conn);
				cmd.Parameters.AddWithValue("@p", mainPartyType);
				object o = cmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					return Convert.ToInt32(o);
				}
			}
			catch (Exception ex)
			{
				Log.Error("EnsureDefaultSubParty Error:{0}", ex.Message);
			}
			return 0;
		}

		private static void competitionEvent_partyUserInfo(int UserNum, out int SubPartyType, out List<CompetitionPartyUserInfo> CompetitionPartyInfo)
		{
			SubPartyType = 0;
			CompetitionPartyInfo = new List<CompetitionPartyUserInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_competitionEvent_partyUserInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					CompetitionPartyUserInfo item = new CompetitionPartyUserInfo
					{
						point = Convert.ToInt32(mySqlDataReader["point"]),
						accPoint = Convert.ToInt32(mySqlDataReader["accPoint"]),
						rewardLevel = Convert.ToByte(mySqlDataReader["rewardLevel"]),
						eventType = Convert.ToByte(mySqlDataReader["eventType"]),
						ReceivedSeasonPassRewardLevel = Convert.ToByte(mySqlDataReader["ReceivedSeasonPassRewardLevel"])
					};
					SubPartyType = Convert.ToInt32(mySqlDataReader["SubPartyType"]);
					CompetitionPartyInfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_competitionEvent_partyUserInfo Error:{0}", ex.Message);
			}
		}

		private static void competitionEvent_partyInfo(out List<CompetitionPartyInfo> CompetitionPartyInfo)
		{
			CompetitionPartyInfo = new List<CompetitionPartyInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_competitionEvent_partyInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					CompetitionPartyInfo item = new CompetitionPartyInfo
					{
						partyType = Convert.ToInt32(mySqlDataReader["partyType"]),
						point = mySqlDataReader.GetInt64("point"),
						userDiffPercent = mySqlDataReader.GetInt32("userDiffPercent")
					};
					CompetitionPartyInfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_competitionEvent_partyInfo Error:{0}", ex.Message);
			}
		}

		private static void competitionEvent_pointReward(int UserNum, short rewardLevel, bool bUseSeasonPass, out List<ExchangeItemInfo> exinfo, out eCompetitionEventResult result)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_competitionEvent_pointReward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("eventType", MySqlDbType.Int16).Value = ServerSettingHolder.ServerSettings.useCompetitionEvent;
				mySqlCommand.Parameters.Add("rewardLevel", MySqlDbType.Int16).Value = rewardLevel;
				mySqlCommand.Parameters.Add("useSeasonPass", MySqlDbType.Int32).Value = bUseSeasonPass;
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
				result = eCompetitionEventResult.eCompetitionEventResult_OK;
			}
			catch (MySqlException ex)
			{
				result = eCompetitionEventResult.eCompetitionEventResult_DBERROR;
				if (ex.Message.Contains("already recv point reward"))
				{
					result = eCompetitionEventResult.eCompetitionEventResult_ALREADY_RECV_POINT_REWARD;
				}
				else if (ex.Message.Contains("not next point reward"))
				{
					result = eCompetitionEventResult.eCompetitionEventResult_NOT_NEXT_POINT_REWARD;
				}
				else if (ex.Message.Contains("not enough point"))
				{
					result = eCompetitionEventResult.eCompetitionEventResult_NOT_ENOUGH_POINT;
				}
				Log.Error("usp_competitionEvent_pointReward Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = eCompetitionEventResult.eCompetitionEventResult_DBERROR;
				Log.Error("usp_competitionEvent_pointReward Error:{0}", ex2.Message);
			}
		}
	}
}
