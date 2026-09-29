using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Item;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Serilog;

namespace AgentServer.Packet
{
	public class GuildHandle
	{
		public static void Handle_CheckGuildName(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			reader.Offset += 4;
			int num = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(num);
			bool flag = true;
			int num2 = 0;
			Regex regex = new Regex("^[0-9A-Za-z\\u4E00-\\u9FFF]+$");
			if (num < 4 || num > 24 || !regex.IsMatch(text))
			{
				flag = false;
				num2 = 2;
			}
			else
			{
				num2 = CheckGuildName(text);
			}
			if (flag && num2 == 0)
			{
				Client.SendAsync(new CheckGuildNameACK(0, last));
			}
			else if (flag && num2 == 1)
			{
				Client.SendAsync(new CheckGuildNameACK(1, last));
			}
			else if (!flag || num2 == 2 || num2 == 3)
			{
				Client.SendAsync(new CheckGuildNameACK(2, last));
			}
		}

		public static void Handle_MakeGuild(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.Offset += 4;
			int num = reader.ReadLEInt16();
			string guildname = reader.ReadBig5StringSafe(num);
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			bool flag = true;
			if (num < 4 || num > 24)
			{
				flag = false;
			}
			// Allow solo create: party is optional (client often requires a party window,
			// but one member is enough — do not require Partys.GetParty).
			if (!(currentAccount.GuildNum < 0 && flag))
			{
				return;
			}
			if (MakeGuild(guildname, currentAccount.UserNum, out var GuildNum))
			{
				currentAccount.TR -= 35000L;
				currentAccount.GuildNum = GuildNum;
				Client.SendAsync(new MakeGuildOKACK(currentAccount.TR, last));
				if (GetGuildInfo(currentAccount.NickName, out var info))
				{
					Client.SendAsync(new GuildInFoACK(isSelf: true, info, last));
				}
				currentAccount.GuildInfo = info;
				if (GetGuildUserInfo(currentAccount.UserNum, out var userinfo))
				{
					Client.SendAsync(new GetGuildUserInfoACK(userinfo, last));
				}
				currentAccount.GuildUserInfo = userinfo;
				if (currentAccount.InGame && room != null)
				{
					ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(currentAccount, currentAccount.NickName, guildname, currentAccount.GuildNum, 5, 0, 1, last), room.RoomServerID);
				}
				if (GetGuildMemberInfo(currentAccount.UserNum, out var memberinfos))
				{
					Client.SendAsync(new CM_GetGuildMemberACK(0, memberinfos, last));
				}
				if (GetGuildFarmInfo(currentAccount.GuildNum, currentAccount.UserNum, out var farminfo))
				{
					Client.SendAsync(new GuildFarmInfoACK(farminfo, last));
				}
			}
			else
			{
				Client.SendAsync(new MakeGuildFailACK(last));
			}
		}

		public static void Handle_DelGuild(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (DelGuild(reader.ReadLEInt32(), currentAccount.UserNum))
			{
				currentAccount.GuildNum = -1;
				Client.SendAsync(new DelGuildOKACK(last));
				NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
				if (currentAccount.InGame && room != null)
				{
					ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(currentAccount, currentAccount.NickName, string.Empty, 0, -1, -1, 0, last), room.RoomServerID);
				}
				Client.SendAsync(new eServer_GET_LADDER_INFO_ACK(last));
				Client.SendAsync(new GuildMissionUserMissionDeleteNotify_ACK(1, last));
				Client.SendAsync(new RemoveChallengingMission_ACK(2, 0, last));
			}
			else
			{
				Client.SendAsync(new DelGuildFailACK(last));
			}
		}

		public static void Handle_GetGuildList(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			reader.Offset += 4;
			short num = 0;
			string text = string.Empty;
			int num2 = reader.ReadLEInt16();
			if (num2 > 0)
			{
				text = reader.ReadBig5StringSafe(num2);
			}
			int num3 = reader.ReadLEInt16();
			if (num3 > 0)
			{
				text = reader.ReadBig5StringSafe(num3);
			}
			num = ((num2 <= 0) ? ((short)1) : ((short)0));
			short limitLevel = reader.ReadLEInt16();
			short guildLevel = reader.ReadLEInt16();
			reader.ReadByte();
			List<GuildInfo> infos;
			int guildList = GetGuildList(limitLevel, guildLevel, 0, num, text, 1, out infos);
			Client.SendAsync(new GuildGetListACK(infos, guildList, num, text, limitLevel, guildLevel, last));
		}

		public static void Handle_RequestJoin(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int guildNum = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string message = reader.ReadBig5StringSafe(fixedLength);
			int reason = 0;
			if (currentAccount.GuildNum < 0 && RequestJoinGuild(guildNum, currentAccount.UserNum, (short)currentAccount.Level, message, out var success, out reason, out var info))
			{
				if (success == 1)
				{
					Client.SendAsync(new RequestJoinGuildInFoACK(info, last));
					if (info.joinMethod == 0)
					{
						currentAccount.GuildNum = info.guildNum;
						currentAccount.GuildInfo = info;
						ItemHandle.getActiveFuncItem(currentAccount, 300);
						if (GetGuildFarmInfo(currentAccount.GuildNum, currentAccount.UserNum, out var farminfo))
						{
							Client.SendAsync(new GuildFarmInfoACK(farminfo, last));
						}
						NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
						if (currentAccount.InGame && room != null)
						{
							ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(currentAccount, currentAccount.NickName, info.guildName, info.guildNum, -1, 0, 1, last), room.RoomServerID);
						}
					}
					return;
				}
				switch (reason)
				{
				case 1:
					Client.SendAsync(new RequestJoinGuildFailACK(14, last));
					break;
				case 2:
					Client.SendAsync(new RequestJoinGuildFailACK(5, last));
					break;
				case 3:
					Client.SendAsync(new RequestJoinGuildFailACK(7, last));
					break;
				case 4:
					Client.SendAsync(new RequestJoinGuildFailACK(4, last));
					break;
				case 5:
					Client.SendAsync(new RequestJoinGuildFailACK(6, last));
					break;
				default:
					Client.SendAsync(new RequestJoinGuildFailACK(2, last));
					break;
				}
			}
			else if (reason == 5)
			{
				Client.SendAsync(new RequestJoinGuildFailACK(6, last));
			}
			else
			{
				Client.SendAsync(new RequestJoinGuildFailACK(2, last));
			}
		}

		public static void Handle_CancelPropose(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			string guildname = CancelPropose(currentAccount.UserNum, num);
			Client.SendAsync(new GuildCancelProposeACK(guildname, num, last));
		}

		public static void Handle_ProcessJoinRequest(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			byte b = reader.ReadByte();
			if (ProcessJoinRequest(currentAccount.UserNum, text, b, out var isOnline, out var guildinfo, out var memberinfo))
			{
				Client.SendAsync(new GuildProcessJoinRequestACK(text, b, memberinfo, last));
				if (isOnline)
				{
					ServerStatus.LBServerActor.Tell(new Guild_ProcessJoinRequest
					{
						NickName = text,
						Accept = (b == 1),
						guildName = guildinfo.guildName,
						guildNum = guildinfo.guildNum
					});
				}
			}
			else
			{
				Client.SendAsync(new GuildProcessJoinRequestFailACK(1, last));
			}
		}

		public static void Handle_ProcessLeave(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			byte b = reader.ReadByte();
			string guildName = currentAccount.GuildInfo.guildName;
			bool isOnline;
			if (b == 1 && currentAccount.GuildUserInfo.grade == 5)
			{
				Client.SendAsync(new GuildProcessLeaveFailACK(last));
			}
			else if (currentAccount.GuildNum > 0 && ProcessLeave(currentAccount.UserNum, text, b, out isOnline))
			{
				Client.SendAsync(new GuildProcessLeaveACK(text, b, last));
				ServerStatus.LBServerActor.Tell(new Guild_ProcessLeave
				{
					NickName = text,
					bySelf = b,
					guildName = guildName
				});
			}
		}

		public static void Handle_GetJoinRequestList(ClientConnection Client, byte last)
		{
			GetJoinRequestList(Client.CurrentAccount.UserNum, out var memberinfos);
			Client.SendAsync(new GuildJoinRequestListACK(memberinfos, last));
		}

		public static void Handle_ModifyJoinLimitLevel(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short joinLimitLevel = reader.ReadLEInt16();
			if (currentAccount.GuildUserInfo.grade == 5)
			{
				ModifyJoinLimitLevel(currentAccount.UserNum, joinLimitLevel);
				Client.SendAsync(new ModifyJoinLimitLevelACK(joinLimitLevel, last));
			}
			else
			{
				Client.SendAsync(new ModifyJoinLimitLevelFailACK(last));
			}
		}

		public static void Handle_ModifyJoinMethod(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short joinMethod = reader.ReadLEInt16();
			if (currentAccount.GuildUserInfo.grade == 5 && ModifyJoinMethod(currentAccount.UserNum, joinMethod))
			{
				Client.SendAsync(new ModifyJoinMethodACK(joinMethod, last));
			}
			else
			{
				Client.SendAsync(new ModifyJoinMethodFailACK(last));
			}
		}

		public static void Handle_ModifyMemberGrade(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string NickName = reader.ReadBig5StringSafe(fixedLength);
			short num = reader.ReadLEInt16();
			if (currentAccount.GuildUserInfo.grade == 5 && num < 5 && ModifyMemberGrade(currentAccount.UserNum, NickName, num))
			{
				Client.SendAsync(new ModifyMemberGradeACK(NickName, num, last));
				Account value = ClientConnection.CurrentAccounts.FirstOrDefault((KeyValuePair<int, Account> f) => f.Value.NickName == NickName).Value;
				if (value != null)
				{
					value.GuildUserInfo.grade = num;
					value.Connection.SendAsync(new GuildMemberUpdateACK(19, value.NickName, num, last));
					NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
					if (value.InGame && room != null)
					{
						ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(value, value.NickName, value.GuildInfo.guildName, value.GuildNum, value.GuildUserInfo.grade, 0, 1, last), room.RoomServerID);
					}
				}
			}
			else
			{
				Client.SendAsync(new ModifyMemberGradeFailACK(last));
			}
		}

		public static void Handle_ModifyMessage(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt16();
			string message = reader.ReadBig5StringSafeNL(num);
			bool flag = true;
			if (num > 100)
			{
				flag = false;
			}
			if (flag)
			{
				if (currentAccount.GuildUserInfo.grade == 5)
				{
					ModifyMessage(currentAccount.UserNum, message);
					Client.SendAsync(new ModifyMessageACK(message, last));
				}
				else
				{
					Client.SendAsync(new ModifyMessageFailACK(last));
				}
			}
		}

		public static void Handle_LevelUP(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (currentAccount.GuildNum > 0 && Guild_LevelUP(currentAccount.UserNum, out var _))
			{
				ServerStatus.LBServerActor.Tell(new Guild_LVUP
				{
					NickName = currentAccount.NickName
				});
			}
		}

		public static void Handle_GetGuildInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			if (GetGuildInfo(text, out var info))
			{
				bool flag = currentAccount.NickName == text;
				if (flag)
				{
					currentAccount.GuildInfo = info;
				}
				Client.SendAsync(new GuildInFoACK(flag, info, last));
			}
		}

		public static void Handle_GetContributionPoint(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int point = GetContributionPoint(guildNum: reader.ReadLEInt32(), UserNum: currentAccount.UserNum);
			Client.SendAsync(new GuildGetContributionPointACK(point, last));
		}

		public static void Handle_GetGuildPoint(ClientConnection Client, PacketReader reader, byte last)
		{
			long guildPoint = GetGuildPoint(reader.ReadLEInt32());
			Client.SendAsync(new GuildGetPointACK(guildPoint, last));
		}

		public static void Handle_UseGiftBox(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int giftBoxItemNum = reader.ReadLEInt32();
			int num = reader.ReadLEInt16();
			string memo = string.Empty;
			if (num > 0)
			{
				memo = reader.ReadBig5StringSafe(num);
			}
			if (useGiftBox(currentAccount.UserNum, giftBoxItemNum, memo, out var receivedItemNum, out var remainItemCount))
			{
				Client.SendAsync(new Guild_UseGiftBoxACK(receivedItemNum, giftBoxItemNum, remainItemCount, last));
			}
		}

		public static void Handle_AddGuildSkill(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			string text = string.Empty;
			string text2 = string.Empty;
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadLEInt32();
				byte b = reader.ReadByte();
				text += $"{num2},";
				text2 += $"{b},";
			}
			if (currentAccount.GuildUserInfo.grade != 5)
			{
				Client.SendAsync(new Guild_AddGuildSkill_Fail_ACK(6, last));
			}
			else if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				long guildPoint;
				byte guildSkillPoint;
				List<GuildSkillInfo> infos;
				Dictionary<int, List<ItemAttr>> SkillAttr;
				int num3 = addGuildSkill(currentAccount.UserNum, text, text2, out guildPoint, out guildSkillPoint, out infos, out SkillAttr);
				if (num3 == 1)
				{
					ServerStatus.LBServerActor.Tell(new Guild_Skill
					{
						guildNum = currentAccount.GuildNum,
						Type = 1
					});
					currentAccount.GuildInfo.point = guildPoint;
					currentAccount.GuildInfo.SkillPoint = guildSkillPoint;
					currentAccount.GuildInfo.SkillInfos = infos;
					Client.SendAsync(new Guild_AddGuildSkill_ACK(guildPoint, guildSkillPoint, infos, last));
				}
				else
				{
					Client.SendAsync(new Guild_AddGuildSkill_Fail_ACK(num3, last));
				}
			}
		}

		public static void Handle_ResetGuildSkill(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (resetGuildSkill(currentAccount.UserNum, out var guildPoint, out var guildSkillPoint, out var itemNnums))
			{
				ServerStatus.LBServerActor.Tell(new Guild_Skill
				{
					guildNum = currentAccount.GuildNum,
					Type = 2,
					items = itemNnums
				});
				Client.SendAsync(new Guild_ResetGuildSkill_ACK(guildPoint, guildSkillPoint, last));
			}
			else
			{
				Client.SendAsync(new Guild_ResetGuildSkill_Fail_ACK(5, last));
			}
		}

		private static int CheckGuildName(string name)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_checkValidGuildName1";
				mySqlCommand.Parameters.Add("guildName", MySqlDbType.VarString).Value = name;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				return Convert.ToInt32(mySqlDataReader["ret"]);
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_checkValidGuildName1 error: {0}", ex.Message);
				return 2;
			}
		}

		private static bool MakeGuild(string guildname, int UserNum, out int GuildNum)
		{
			GuildNum = -1;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_makeGuild";
					mySqlCommand.Parameters.Add("guildName", MySqlDbType.VarString).Value = guildname;
					mySqlCommand.Parameters.Add("guildMasterUserNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					int num = Convert.ToInt32(mySqlDataReader["ret"]);
					GuildNum = Convert.ToInt32(mySqlDataReader["guildNum"]);
					if (num == 0)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_makeGuild error: {0}", ex.Message);
				return false;
			}
		}

		private static bool DelGuild(int guildnum, int UserNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_delGuild";
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildnum;
					mySqlCommand.Parameters.Add("masterUserNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_delGuild error: {0}", ex.Message);
				return false;
			}
		}

		public static bool GetGuildInfo(string NickName, out GuildInfo info)
		{
			info = new GuildInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_getGuildInfo";
					mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = NickName;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						info.guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]);
						info.kind = Convert.ToInt16(mySqlDataReader["kind"]);
						info.mark = Convert.ToInt32(mySqlDataReader["mark"]);
						info.guildName = mySqlDataReader["guildName"].ToString();
						info.masterName = mySqlDataReader["masterName"].ToString();
						info.foundationDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["foundationDate"]));
						info.memberCount = Convert.ToInt16(mySqlDataReader["memberCount"]);
						info.memberLimit = Convert.ToInt16(mySqlDataReader["memberLimit"]);
						info.exp = Convert.ToInt64(mySqlDataReader["exp"]);
						info.nextLevelExp = Convert.ToInt64(mySqlDataReader["nextLevelExp"]);
						info.point = Convert.ToInt64(mySqlDataReader["point"]);
						info.ladderPoint = Convert.ToInt32(mySqlDataReader["ladderPoint"]);
						info.joinLimitLevelOver = Convert.ToInt32(mySqlDataReader["joinLimitLevelOver"]);
						info.joinLimitLevelBelow = Convert.ToInt32(mySqlDataReader["joinLimitLevelBelow"]);
						info.joinMethod = Convert.ToInt32(mySqlDataReader["joinMethod"]);
						info.level = Convert.ToInt32(mySqlDataReader["level"]);
						info.message = mySqlDataReader["message"].ToString();
						info.attendanceCount = Convert.ToInt32(mySqlDataReader["attendanceCount"]);
						info.SkillPoint = mySqlDataReader.GetByte("skillPoint");
						mySqlDataReader.NextResult();
						while (mySqlDataReader.Read())
						{
							GuildSkillInfo item = new GuildSkillInfo
							{
								SkillNum = mySqlDataReader.GetInt32("fdSkillNum"),
								SkillLevel = mySqlDataReader.GetByte("fdSkillLevel")
							};
							info.SkillInfos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getGuildInfo error: {0}", ex.Message);
				return false;
			}
		}

		public static bool GetGuildFarmInfo(int GuildNum, int UserNum, out GuildFarmInfo farminfo)
		{
			farminfo = new GuildFarmInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_GetGuildFarmInfo";
					mySqlCommand.Parameters.Add("pGuildNum", MySqlDbType.Int32).Value = GuildNum;
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						farminfo.FarmIndex = Convert.ToInt32(mySqlDataReader["FarmIndex"]);
						farminfo.FarmTypeNum = Convert.ToInt32(mySqlDataReader["FarmTypeNum"]);
						farminfo.FarmName = mySqlDataReader["FarmName"].ToString();
						farminfo.ExpireDateTime = ((mySqlDataReader["ExpireDateTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ExpireDateTime"])));
						farminfo.CreateDateTime = ((mySqlDataReader["CreateDateTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["CreateDateTime"])));
						farminfo.Password = mySqlDataReader["Password"].ToString();
						farminfo.TotalVisitedCount = Convert.ToInt32(mySqlDataReader["TotalVisitedCount"]);
						farminfo.TodaysVisitorCount = Convert.ToInt32(mySqlDataReader["TodaysVisitorCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetGuildFarmInfo error: {0}", ex.Message);
				return false;
			}
		}

		public static bool GetGuildUserInfo(int UserNum, out GuildUserInfo userinfo)
		{
			userinfo = new GuildUserInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_GetUserInfo";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						userinfo.guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]);
						userinfo.guildKind = Convert.ToInt32(mySqlDataReader["guildKind"]);
						userinfo.grade = Convert.ToInt32(mySqlDataReader["grade"]);
						userinfo.contributionPoint = Convert.ToInt32(mySqlDataReader["contributionPoint"]);
						userinfo.joinDate = ((mySqlDataReader["joinDate"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["joinDate"])));
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_GetUserInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool RequestJoinGuild(int guildNum, int requestedUserNum, short requestedUserlevel, string message, out int success, out int reason, out GuildInfo info)
		{
			success = 0;
			reason = 0;
			info = new GuildInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_requestJoin";
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
					mySqlCommand.Parameters.Add("requestedUserNum", MySqlDbType.Int32).Value = requestedUserNum;
					mySqlCommand.Parameters.Add("requestedUserlevel", MySqlDbType.Int16).Value = requestedUserlevel;
					mySqlCommand.Parameters.Add("message", MySqlDbType.VarString).Value = message;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						success = Convert.ToInt32(mySqlDataReader["success"]);
						if (success == 1)
						{
							info.guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]);
							info.kind = Convert.ToInt16(mySqlDataReader["kind"]);
							info.mark = Convert.ToInt32(mySqlDataReader["mark"]);
							info.guildName = mySqlDataReader["guildName"].ToString();
							info.masterName = mySqlDataReader["masterName"].ToString();
							info.foundationDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["foundationDate"]));
							info.memberCount = Convert.ToInt16(mySqlDataReader["memberCount"]);
							info.memberLimit = Convert.ToInt16(mySqlDataReader["memberLimit"]);
							info.exp = Convert.ToInt64(mySqlDataReader["exp"]);
							info.nextLevelExp = Convert.ToInt64(mySqlDataReader["nextLevelExp"]);
							info.point = Convert.ToInt64(mySqlDataReader["point"]);
							info.ladderPoint = Convert.ToInt32(mySqlDataReader["ladderPoint"]);
							info.joinLimitLevelOver = Convert.ToInt32(mySqlDataReader["joinLimitLevelOver"]);
							info.joinLimitLevelBelow = Convert.ToInt32(mySqlDataReader["joinLimitLevelBelow"]);
							info.joinMethod = Convert.ToInt32(mySqlDataReader["joinMethod"]);
							info.level = Convert.ToInt32(mySqlDataReader["level"]);
							info.message = mySqlDataReader["message"].ToString();
							info.attendanceCount = Convert.ToInt32(mySqlDataReader["attendanceCount"]);
							info.SkillPoint = mySqlDataReader.GetByte("skillPoint");
							mySqlDataReader.NextResult();
							while (mySqlDataReader.Read())
							{
								GuildSkillInfo item = new GuildSkillInfo
								{
									SkillNum = mySqlDataReader.GetInt32("fdSkillNum"),
									SkillLevel = mySqlDataReader.GetByte("fdSkillLevel")
								};
								info.SkillInfos.Add(item);
							}
						}
						else
						{
							reason = Convert.ToInt32(mySqlDataReader["reason"]);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				if (ex.Message.ToLower().Contains("duplicate entry"))
				{
					reason = 5;
				}
				else
				{
					Log.Error("usp_guild_requestJoin error: {0}, guildNum:{1}, requestedUserNum:{2}", ex.Message, guildNum, requestedUserNum);
				}
				return false;
			}
		}

		private static void ModifyJoinLimitLevel(int UserNum, short joinLimitLevel)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_modifyJoinLimitLevel";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("joinLimitLevel", MySqlDbType.Int16).Value = joinLimitLevel;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_modifyJoinLimitLevel error: {0}", ex.Message);
			}
		}

		private static bool ModifyJoinMethod(int UserNum, short joinMethod)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_modifyJoinMethod";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("joinMethod", MySqlDbType.Int32).Value = joinMethod;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_modifyJoinMethod error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ModifyMemberGrade(int UserNum, string memberNickName, short grade)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_modifyMemberGrade";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("memberNickName", MySqlDbType.VarString).Value = memberNickName;
					mySqlCommand.Parameters.Add("grade", MySqlDbType.Int16).Value = grade;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_modifyMemberGrade error: {0}", ex.Message);
				return false;
			}
		}

		private static void ModifyMessage(int UserNum, string Message)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_modifyMessage";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("Message", MySqlDbType.VarString).Value = Message;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_modifyMessage error: {0}", ex.Message);
			}
		}

		private static bool Guild_LevelUP(int UserNum, out int guildNum)
		{
			guildNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_levelUp";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_levelUp error: {0}", ex.Message);
				return false;
			}
		}

		public static bool GetGuildMemberInfo(int UserNum, out List<GuildMemberInfo> memberinfos)
		{
			memberinfos = new List<GuildMemberInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_cm_guildMemberGet";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
					MySqlDataReader reader = mySqlCommand.ExecuteReader();
					try
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								GuildMemberInfo item = new GuildMemberInfo
								{
									nickname = reader["nickname"].ToString(),
									FarmUniqueNum = Convert.ToInt32(reader["FarmUniqueNum"]),
									ExpireDateTime = ((reader["ExpireDateTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["ExpireDateTime"]))),
									lastLogoutTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["lastLogoutTime"])),
									joinDate = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["joinDate"])),
									grade = Convert.ToInt16(reader["grade"]),
									contributionPoint = Convert.ToInt32(reader["contributionPoint"]),
									Level = (short)(AccountHolder.LevelInfo.Count((long c) => c <= Convert.ToInt64(reader["exp"])) + 1),
									memo = reader.GetString("memo")
								};
								memberinfos.Add(item);
							}
							return true;
						}
					}
					finally
					{
						if (reader != null)
						{
							((IDisposable)reader).Dispose();
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_cm_guildMemberGet error: {0}", ex.Message);
				return false;
			}
		}

		private static void GetJoinRequestList(int UserNum, out List<GuildJoinRequestInfo> memberinfos)
		{
			memberinfos = new List<GuildJoinRequestInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_getJoinRequestList";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildJoinRequestInfo item = new GuildJoinRequestInfo
					{
						nickName = mySqlDataReader["nickName"].ToString(),
						date = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["date"])),
						message = mySqlDataReader["message"].ToString(),
						exp = Convert.ToInt64(mySqlDataReader["exp"])
					};
					memberinfos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getJoinRequestList error: {0}", ex.Message);
			}
		}

		private static int GetGuildList(short limitLevel, short guildLevel, short guildKind, short searchArgumentType, string searchArgumentValues, int totalCount, out List<GuildInfo> infos)
		{
			int result = 0;
			infos = new List<GuildInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_getGuildList";
				mySqlCommand.Parameters.Add("limitLevel", MySqlDbType.Int16).Value = limitLevel;
				mySqlCommand.Parameters.Add("guildLevel", MySqlDbType.Int16).Value = guildLevel;
				mySqlCommand.Parameters.Add("guildKind", MySqlDbType.Int16).Value = guildKind;
				mySqlCommand.Parameters.Add("searchArgumentType", MySqlDbType.Int16).Value = searchArgumentType;
				mySqlCommand.Parameters.Add("searchArgumentValues", MySqlDbType.VarString).Value = searchArgumentValues;
				mySqlCommand.Parameters.Add("totalCount", MySqlDbType.Int32).Value = totalCount;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildInfo item = new GuildInfo
					{
						guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]),
						mark = Convert.ToInt32(mySqlDataReader["mark"]),
						guildName = mySqlDataReader["guildName"].ToString(),
						masterName = mySqlDataReader["masterName"].ToString(),
						foundationDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["foundationDate"])),
						memberCount = Convert.ToInt16(mySqlDataReader["memberCount"]),
						memberLimit = Convert.ToInt16(mySqlDataReader["memberLimitCount"]),
						joinLimitLevelOver = Convert.ToInt32(mySqlDataReader["joinLimitLevelOver"]),
						joinLimitLevelBelow = Convert.ToInt32(mySqlDataReader["joinLimitLevelBelow"]),
						joinMethod = Convert.ToInt32(mySqlDataReader["joinMethod"]),
						level = Convert.ToInt32(mySqlDataReader["level"]),
						message = mySqlDataReader["message"].ToString()
					};
					result = Convert.ToInt32(mySqlDataReader["totalCount"]);
					infos.Add(item);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getGuildList error: {0}", ex.Message);
				return result;
			}
		}

		private static bool ProcessJoinRequest(int masterUserNum, string waitingUserNickName, byte accept, out bool isOnline, out GuildInfo2 guildinfo, out GuildMemberInfo memberinfo)
		{
			isOnline = false;
			memberinfo = null;
			guildinfo = new GuildInfo2();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_processJoinRequest";
					mySqlCommand.Parameters.Add("masterUserNum", MySqlDbType.Int32).Value = masterUserNum;
					mySqlCommand.Parameters.Add("waitingUserNickName", MySqlDbType.VarString).Value = waitingUserNickName;
					mySqlCommand.Parameters.Add("accept", MySqlDbType.Int32).Value = accept;
					MySqlDataReader reader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					try
					{
						if (reader.HasRows)
						{
							reader.Read();
							isOnline = Convert.ToInt32(reader["waitingUserloginServerNum"]) > 0;
							if (accept == 1)
							{
								memberinfo = new GuildMemberInfo
								{
									nickname = reader["nickname"].ToString(),
									FarmUniqueNum = Convert.ToInt32(reader["FarmUniqueNum"]),
									ExpireDateTime = ((reader["ExpireDateTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["ExpireDateTime"]))),
									lastLogoutTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["lastLogoutTime"])),
									joinDate = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["joinDate"])),
									grade = Convert.ToInt16(reader["grade"]),
									contributionPoint = Convert.ToInt32(reader["contributionPoint"]),
									Level = (short)(AccountHolder.LevelInfo.Count((long c) => c <= Convert.ToInt64(reader["exp"])) + 1)
								};
							}
							guildinfo.guildName = reader["guildName"].ToString();
							guildinfo.guildNum = Convert.ToInt32(reader["guildNum"]);
							return true;
						}
					}
					finally
					{
						if (reader != null)
						{
							((IDisposable)reader).Dispose();
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_processJoinRequest error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ProcessLeave(int UserNum, string leavingUserNickName, byte bySelf, out bool isOnline)
		{
			isOnline = false;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_processLeave";
					mySqlCommand.Parameters.Add("requestUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("leavingUserNickName", MySqlDbType.VarString).Value = leavingUserNickName;
					mySqlCommand.Parameters.Add("bySelf", MySqlDbType.Int32).Value = bySelf;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						isOnline = Convert.ToInt32(mySqlDataReader["leavingUserloginServerNum"]) > 0;
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_processLeave error: {0}", ex.Message);
				return false;
			}
		}

		private static int GetContributionPoint(int UserNum, int guildNum)
		{
			int result = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_getContributionPoint";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows && mySqlDataReader.Read())
					{
						result = mySqlDataReader.GetInt32("contributionPoint");
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getContributionPoint error: {0}", ex.Message);
				return result;
			}
		}

		private static long GetGuildPoint(int guildNum)
		{
			long result = 0L;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_getGuildPoint";
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows && mySqlDataReader.Read())
					{
						result = mySqlDataReader.GetInt64("guildPoint");
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getGuildPoint error: {0}", ex.Message);
				return result;
			}
		}

		private static bool useGiftBox(int UserNum, int giftBoxItemNum, string memo, out int receivedItemNum, out int remainItemCount)
		{
			receivedItemNum = 0;
			remainItemCount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_useGiftBox";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("giftBoxItemNum", MySqlDbType.Int32).Value = giftBoxItemNum;
					mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						receivedItemNum = mySqlDataReader.GetInt32("receivedItemNum");
						remainItemCount = Convert.ToInt32(mySqlDataReader["remainItemCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_useGiftBox error: {0}", ex.Message);
				return false;
			}
		}

		public static void GetProposeList(int userNum, out List<GuildInfo> infos)
		{
			infos = new List<GuildInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guild_getProposeList";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildInfo item = new GuildInfo
					{
						guildNum = Convert.ToInt32(mySqlDataReader["guildNum"]),
						mark = Convert.ToInt32(mySqlDataReader["mark"]),
						guildName = mySqlDataReader["guildName"].ToString(),
						masterName = mySqlDataReader["masterName"].ToString(),
						foundationDate = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["foundationDate"])),
						memberCount = Convert.ToInt16(mySqlDataReader["memberCount"]),
						memberLimit = Convert.ToInt16(mySqlDataReader["memberLimitCount"]),
						joinLimitLevelOver = Convert.ToInt32(mySqlDataReader["joinLimitLevelOver"]),
						joinLimitLevelBelow = Convert.ToInt32(mySqlDataReader["joinLimitLevelBelow"]),
						joinMethod = Convert.ToInt32(mySqlDataReader["joinMethod"]),
						level = Convert.ToInt32(mySqlDataReader["level"]),
						message = mySqlDataReader["message"].ToString()
					};
					infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_getProposeList error: {0}", ex.Message);
			}
		}

		private static string CancelPropose(int userNum, int guildNum)
		{
			string result = string.Empty;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_cancelPropose";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows && mySqlDataReader.Read())
					{
						result = mySqlDataReader.GetString("guildName");
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("\tusp_guild_cancelPropose error: {0}", ex.Message);
				return result;
			}
		}

		private static int addGuildSkill(int userNum, string guildSkillNumList, string guildSkillLevelNumList, out long guildPoint, out byte guildSkillPoint, out List<GuildSkillInfo> infos, out Dictionary<int, List<ItemAttr>> SkillAttr)
		{
			guildPoint = 0L;
			guildSkillPoint = 0;
			infos = new List<GuildSkillInfo>();
			SkillAttr = new Dictionary<int, List<ItemAttr>>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_addGuildSkill";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("guildSkillNumList", MySqlDbType.VarString).Value = guildSkillNumList;
					mySqlCommand.Parameters.Add("guildSkillLevelNumList", MySqlDbType.VarString).Value = guildSkillLevelNumList;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						int @int = mySqlDataReader.GetInt32("ret");
						if (@int == 1)
						{
							guildPoint = Convert.ToInt64(mySqlDataReader["guildPoint"]);
							guildSkillPoint = Convert.ToByte(mySqlDataReader["guildSkillPoint"]);
							mySqlDataReader.NextResult();
							while (mySqlDataReader.Read())
							{
								GuildSkillInfo item = new GuildSkillInfo
								{
									SkillNum = mySqlDataReader.GetInt32("fdSkillNum"),
									SkillLevel = mySqlDataReader.GetByte("fdSkillLevel")
								};
								infos.Add(item);
							}
							mySqlDataReader.NextResult();
							while (mySqlDataReader.Read())
							{
								int int2 = mySqlDataReader.GetInt32("fdItemNum");
								ItemAttr item2 = new ItemAttr
								{
									Attr = Convert.ToUInt16(mySqlDataReader["fdAttrType"]),
									AttrValue = Convert.ToSingle(mySqlDataReader["fdAttrValue"])
								};
								if (!SkillAttr.ContainsKey(int2))
								{
									SkillAttr.Add(int2, new List<ItemAttr> { item2 });
								}
								else
								{
									SkillAttr[int2].Add(item2);
								}
							}
						}
						return @int;
					}
				}
				return 0;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_addGuildSkill error: {0}", ex.Message);
				return 0;
			}
		}

		private static bool resetGuildSkill(int userNum, out long guildPoint, out byte guildSkillPoint, out List<int> itemNnums)
		{
			guildPoint = 0L;
			guildSkillPoint = 0;
			itemNnums = new List<int>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_guild_resetGuildSkill";
					mySqlCommand.Parameters.Add("masterUserNum", MySqlDbType.Int32).Value = userNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						guildPoint = mySqlDataReader.GetInt64("fdPoint");
						guildSkillPoint = mySqlDataReader.GetByte("fdSkillPoint");
						mySqlDataReader.NextResult();
						while (mySqlDataReader.Read())
						{
							itemNnums.Add(mySqlDataReader.GetInt32("fdSkillItemDescNum"));
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_guild_resetGuildSkill error: {0}", ex.Message);
				return false;
			}
		}
	}
}
