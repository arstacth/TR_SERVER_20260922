using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentServer.Database;
using AgentServer.EasyAntiCheat;
using AgentServer.Network.Connections;
using AgentServer.Packet;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Shu;
using AgentServer.Structuring.User;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Quartz;
using Serilog;
using TRCommon;

namespace AgentServer.Holders
{
	public static class LoginTrafficManager
	{
		public static ConcurrentDictionary<int, ReLoginUserInfo> mapWaitLoginUserList = new ConcurrentDictionary<int, ReLoginUserInfo>();

		public static ConcurrentDictionary<int, string> mapWaitLoginUserIDList = new ConcurrentDictionary<int, string>();

		public static ConcurrentQueue<long> queueLoginTimeList = new ConcurrentQueue<long>();

		public static ConcurrentQueue<long> queueLogoutTimeList = new ConcurrentQueue<long>();

		private static long m_tLastLoginQueueUpdateTime;

		private static IActorRef LoginTrafficActor;

		public static void Init()
		{
			if (Conf.useLoginTrafficManager)
			{
				LoginTrafficActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new LoginTrafficActor()), "LoginTrafficManager");
				ServerStatus.QuartzActor.Tell(new CreateJob(LoginTrafficActor, "", TriggerBuilder.Create().WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
				{
					x.WithIntervalInSeconds(1).RepeatForever();
				}).Build()));
			}
		}

		public static bool canAcceptLogin()
		{
			return ClientConnection.TotalAgentLoginUser < Conf.MaxTotalAgentUserCount;
		}

		public static void insertWaitLoginUserInfo(ReLoginUserInfo userInfo)
		{
			mapWaitLoginUserList.TryAdd(userInfo.Session, userInfo);
			mapWaitLoginUserIDList.TryAdd(userInfo.Session, userInfo.strID);
		}

		public static float getLoginUserNumPerMinute()
		{
			return queueLoginTimeList.Count;
		}

		public static float getLogoutUserNumPerMinute()
		{
			return queueLogoutTimeList.Count;
		}

		public static int getLoginWaitUserNum()
		{
			return mapWaitLoginUserList.Count;
		}

		public static int getAverageWaitTimePerOneUser()
		{
			return 600;
		}

		public static void updateLoginTimeQueue()
		{
			m_tLastLoginQueueUpdateTime = Utility.CurrentTimeMilliseconds();
			queueLoginTimeList.Enqueue(m_tLastLoginQueueUpdateTime);
		}

		public static void updateLogoutTimeQueue()
		{
			queueLogoutTimeList.Enqueue(Utility.CurrentTimeMilliseconds());
		}

		public static void LoginProcess(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			currentAccount.isWaitLogin = false;
			ServerStatus.LBServerActor.Tell(new OnlineUserUpdate
			{
				OnlineCount = ClientConnection.CurrentAccounts.Count,
				LoginedCount = ClientConnection.CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin)
			});
			if (!RequestLogin(currentAccount, out var failReason, out var logininfo))
			{
				currentAccount.isBlocked = true;
				switch (failReason)
				{
				case eLoginFail.eLoginFail_BLACK_LIST:
				{
					GetBlackListInfo(logininfo.UserNum, out var blockinfo);
					Client.SendAsync(new LoginBlackList(blockinfo.Item1, blockinfo.Item2, blockinfo.Item3, last));
					break;
				}
				case eLoginFail.eLoginFail_INVALID_COUNTRY:
				case eLoginFail.eLoginFail_BAD_NUMBER_FOR_PREVENT_ABUSING:
					Client.SendAsync(new LoginError(12, last, (byte)failReason));
					break;
				default:
					Client.SendAsync(new LoginError(9, last, 0));
					break;
				}
				return;
			}
			EACServer.OnJoinGame(Client);
			getNickname(currentAccount);
			getUserCash(currentAccount);
			CompetitionEventHandle.LoadPartyOntoAccount(currentAccount);
			Client.SendAsync(new NP_Byte(DBInit.GameServerSetting));
			Client.SendAsync(new LoginUserInfo(currentAccount, logininfo, last));
			// Already on Animal Village team: store party on client before EventChannel click.
			CompetitionEventHandle.SendAlreadyJoinedAtLogin(Client, last);
			// Do not send opcode 5. Empty ADD_HACKING_TOOL_HASH_ACK still hits Recv HackingTool
			// and loads HackAlarmPopup (pink 1024_abusereport). EnableAgentHashCheck stays false.
			Client.SendAsync(new NP_Byte(DBInit.SmartChannelModeInfo));
			if (DBInit.SmartChannelScheduleInfo != null && DBInit.SmartChannelScheduleInfo.Length > 0)
			{
				Client.SendAsync(new NP_Byte(DBInit.SmartChannelScheduleInfo));
			}
			// Official login: OnRecvChallengeMapNPCMatchMapInfo (cat18/map5308 = summer event).
			Client.SendAsync(new ChallengeMapNPCMatchMapInfoAck(last));
			Client.SendAsync(new LoginGetNickName_0X1C(currentAccount, last));
			if (!currentAccount.noNickName)
			{
				Client.SendAsync(new LoginFarm_GetMyFarmInfo(currentAccount, last));
				// Official login burst: QuestUserInfo + EventNotify + LobbyQuestUserInfo + LobbyQuestEventNotify.
				Client.SendAsync(new QuestUserInfo_ACK(isUpdate: false, currentAccount, 0, last));
				Client.SendAsync(new QuestEventNotify_ACK(last));
				Client.SendAsync(new GetLobbyQuestUserInfo(last));
				Client.SendAsync(new GetLobbyQuestEventNotify(last));
			}
			// Load confirmed base-stat attrs into memory + push CHARACTER_ATTR_ACK.
			// Without this, fdStatReset is in DB but room/lobby still encode empty char attrs.
			getUserCharacterAttr(currentAccount, last);
			currentAccount.GetMyLevel();
			// Official/2021 login burst: push Shu character list + status before lobby.
			// openShuRoomUI loads character_shu\*\*_set.ca3 from this cache during openUI
			// (~60ms). ExploreCheck-only list arrives after UI type 29 is already open, so
			// the model never loads even when the opcode-2 ACK parses cleanly.
			ShuSystemHandle.Shu_GetUserCharacterItemList(currentAccount, out DBShuInfo shuInfos);
			Client.SendAsync(new Shu_GetUserCharacterItemList(shuInfos, last));
			ShuSystemHandle.Shu_UserStatusInfo(currentAccount, out var shuStatus);
			Client.SendAsync(new Shu_GetUserStatusInfo(shuStatus, last));
			shuInfos.shuavatars.TryGetValue(currentAccount.CurrentShuID, out var shuAvatars);
			shuInfos.shuchars.TryGetValue(currentAccount.CurrentShuID, out var shuChar);
			shuInfos.shustatus.TryGetValue(currentAccount.CurrentShuID, out var shuStat);
			ShuSystemHandle.UpdateUserShuInfo(currentAccount, currentAccount.CurrentShuID, shuAvatars, shuChar, shuStat);
			// debug_trgame "%d Shu MP" = point type 2100. Push so HUD is not left at -1.
			int shuMpHud = shuInfos.remainMP;
			if (shuMpHud < 0)
			{
				shuMpHud = 0;
			}
			if (shuMpHud > 20)
			{
				shuMpHud = 20;
			}
			Client.SendAsync(new GetUserPoint(2100, shuMpHud, shuMpHud, last));
			try
			{
				Directory.CreateDirectory("Logs\\Relay");
				// Write AFTER lobby burst is queued; Relay waits ~0.85s then REGIST_ACK.
				File.WriteAllText("Logs\\Relay\\login_ok.flag", currentAccount.Session.ToString());
			}
			catch
			{
			}
		}

		public static void Logout(int session, string strID, bool bSendToAllAgentServer)
		{
			if (mapWaitLoginUserIDList.TryGetValue(session, out var value) && value == strID)
			{
				mapWaitLoginUserList.TryRemove(session, out var _);
				mapWaitLoginUserIDList.TryRemove(session, out var _);
				if (bSendToAllAgentServer)
				{
					ServerStatus.LBServerActor.Tell(new DelWaitLoginUserInfo
					{
						Info = new Tuple<string, int>(strID, session)
					});
				}
			}
		}

		private static bool RequestLogin(Account User, out eLoginFail failReason, out UserLoginInfo logininfo)
		{
			failReason = eLoginFail.eLoginFail_UNKNOWN;
			logininfo = new UserLoginInfo();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_login");
				mySqlCommandHelper.AddParamInt("servernum", ServerStatus.MyAgentID);
				mySqlCommandHelper.AddParamVarString("userid", User.UserID);
				mySqlCommandHelper.AddParamLong("authnum", -1L);
				mySqlCommandHelper.AddParamInt("loginkey", 123456);
				mySqlCommandHelper.AddParamInt("regpcroom", -1);
				mySqlCommandHelper.AddParamVarString("ip", User.LastIp);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					int num;
					if (mySqlCommandHelper.GetBoolean("blackListCheck"))
					{
						eBlockReason @int = (eBlockReason)mySqlCommandHelper.GetInt("blockReason");
						num = (logininfo.UserNum = (User.UserNum = mySqlCommandHelper.GetInt("usernum")));
						if (eBlockReason.eBlockReason_BAD_NUMBER_FOR_PREVENT_ABUSING == @int)
						{
							failReason = eLoginFail.eLoginFail_BAD_NUMBER_FOR_PREVENT_ABUSING;
						}
						else if (eBlockReason.eBlockReason_REPORT_SYSTEM_PENALTY == @int)
						{
							failReason = eLoginFail.eLoginFail_REPORT_SYSTEM_PENALTY;
						}
						else
						{
							failReason = eLoginFail.eLoginFail_BLACK_LIST;
						}
						return false;
					}
					num = (logininfo.UserNum = (User.UserNum = mySqlCommandHelper.GetInt("userindex")));
					logininfo.nickName = mySqlCommandHelper.GetString("nickName");
					long num2 = (logininfo.TR = (User.TR = mySqlCommandHelper.GetLong("gamemoney")));
					num2 = (logininfo.EXP = (User.Exp = mySqlCommandHelper.GetLong("userexp")));
					User.CoupleInfo.CoupleNum = mySqlCommandHelper.GetInt("couplenum");
					User.CoupleInfo.MateName = mySqlCommandHelper.GetString("matename");
					User.CoupleInfo.CreateTime = mySqlCommandHelper.GetDateTime("createtime", 1842465389770955L);
					User.CoupleInfo.CoupleRingNum = mySqlCommandHelper.GetInt("coupleRingNum");
					User.CoupleInfo.CondDays = mySqlCommandHelper.GetInt("condDays");
					User.CoupleInfo.CoupleLevel = mySqlCommandHelper.GetShort("coupleLevel");
					User.CoupleInfo.MaxRingDays = mySqlCommandHelper.GetInt("maxCoupleRingDays");
					User.CoupleInfo.CoupleType = mySqlCommandHelper.GetInt("coupleType");
					User.CoupleInfo.MarriedTime = mySqlCommandHelper.GetDateTime("marriedDateTime", 1842465389770955L);
					User.CoupleInfo.RingChangedTime = mySqlCommandHelper.GetDateTime("ringChangedTime", 1842465389770955L);
					User.CoupleInfo.AccumulateExp = mySqlCommandHelper.GetInt("accumulateExp");
					User.CoupleInfo.CouplePoint = mySqlCommandHelper.GetInt("couplePoint");
					logininfo.playingTime = mySqlCommandHelper.GetInt("playingTime");
					User.GuildNum = mySqlCommandHelper.GetInt("guildNum");
					num = (logininfo.Attribute = (User.Attribute = mySqlCommandHelper.GetInt("attribute")));
					User.PartyType = mySqlCommandHelper.GetShort("partyType");
					num = (logininfo.GameOption = (User.GameOption = mySqlCommandHelper.GetInt("gameOption")));
					logininfo.shuMP = mySqlCommandHelper.GetInt("shuMP");
					num = (logininfo.TopRank = (User.TopRank = mySqlCommandHelper.GetInt("TopRank")));
					num2 = (logininfo.CurrentShuID = (User.CurrentShuID = mySqlCommandHelper.GetLong("currentShuCharacterItemID")));
					User.LoginKey = mySqlCommandHelper.GetString("loginkey");
					User.MileagePoint = mySqlCommandHelper.GetInt("mileagePoint");
					User.VipLevel = mySqlCommandHelper.GetInt("vipLevel");
					return true;
				}
			}
			catch (MySqlException ex)
			{
				Log.Error("usp_login Error: {0}, userID : {1}", ex.Message, User.UserID);
				if (ex.Message.Contains("in blacklist"))
				{
					failReason = eLoginFail.eLoginFail_BLACK_LIST;
				}
				else if (ex.Message.Contains("invalid country"))
				{
					failReason = eLoginFail.eLoginFail_INVALID_COUNTRY;
				}
			}
			catch (Exception ex2)
			{
				Log.Error("usp_login Error: {0}", ex2.Message);
			}
			return false;
		}

		private static void GetBlackListInfo(int UserNum, out Tuple<int, long, long> blockinfo)
		{
			blockinfo = new Tuple<int, long, long>(0, 0L, 0L);
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getMyBlackListInfo");
				mySqlCommandHelper.AddParamInt("UserNum", UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					int @int = mySqlCommandHelper.GetInt("blockreason");
					long dateTime = mySqlCommandHelper.GetDateTime("blockstarttime", 0L);
					long dateTime2 = mySqlCommandHelper.GetDateTime("blockendtime", 0L);
					blockinfo = new Tuple<int, long, long>(@int, dateTime, dateTime2);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getMyBlackListInfo Error: {0}", ex.Message);
			}
		}

		private static void getNickname(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getNickname");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					if (mySqlCommandHelper.IsDBNull("fdNickname"))
					{
						User.noNickName = true;
						return;
					}
					User.NickName = mySqlCommandHelper.GetString("fdNickname");
					User.noNickName = false;
				}
				else
				{
					User.noNickName = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getNickname Error: {0}", ex.Message);
			}
		}

		private static void getUserCash(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getUserCash");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					User.Cash = mySqlCommandHelper.GetInt("cash");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getUserCash Error: {0}", ex.Message);
			}
			User.CashNeedUpdateFromDB = false;
		}

		public static void getUserItemAttr(Account User, bool bStrengthenGetMyItemLoad = true)
		{
			Dictionary<int, UserItemAttrInfo> dictionary = new Dictionary<int, UserItemAttrInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getUserItemAttr");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					int @int = mySqlCommandHelper.GetInt("ItemDescNum");
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, new UserItemAttrInfo());
					}
					dictionary[@int].m_iItemDescNum = @int;
					dictionary[@int].m_ItemAttr[@short] = @float;
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getUserItemAttr Error: {0}", ex.ToString());
			}
			if (flag)
			{
				User.userItemAttr.fromMap(dictionary);
				if (bStrengthenGetMyItemLoad)
				{
					ArinHandle.DBRequestItemStrengthenGetMyItem(User, -1);
					ArinHandle.GroupItemStrengthenAttr(User);
					return;
				}
				User.Connection.SendAsync(new GET_USER_ITEM_ATTR_ACK(User, 1));
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ITEM_ATTR(User, 1), room.RoomServerID);
				}
			}
			else
			{
				User.Connection.SendAsync(new GET_USER_ITEM_ATTR_ACK(eServerResult.eServerResult_GET_USER_ITEM_ATTR_FAILED_ACK, 1));
			}
		}

		private static void getUserCharacterAttr(Account User, byte last)
		{
			Dictionary<int, UserItemAttrInfo> dictionary = new Dictionary<int, UserItemAttrInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getUserCharacterAttr");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					int @int = mySqlCommandHelper.GetInt("itemDescNum");
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, new UserItemAttrInfo());
					}
					dictionary[@int].m_iItemDescNum = @int;
					dictionary[@int].m_ItemAttr[@short] = @float;
				}
				flag = true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_getUserCharacterAttr Error: {0}", ex.ToString());
			}
			if (flag)
			{
				User.userItemAttr.fromCharMap(dictionary);
				User.Connection.SendAsync(new eServer_GET_USER_CHARACTER_ATTR_ACK(User, last));
			}
		}
	}
}
