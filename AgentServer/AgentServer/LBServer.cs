using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Park;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using LocalCommons.Network;
using NetMsg.LBS;
using Quartz;
using Serilog;

namespace AgentServer
{
	public class LBServer : ReceiveActor
	{
		private static string LBSPath = "akka.tcp://LoadBalance@localhost:{LBSPORT}/user/AgentClient".Replace("{LBSPORT}", Conf.LBSLocalPort.ToString());

		private readonly ActorSelection _lbserver = UntypedActor.Context.ActorSelection(LBSPath);

		public LBServer()
		{
			Receive(delegate(AgentToLBSRequest cr)
			{
				Log.Information("Connecting to LBServer......");
				_lbserver.Tell(cr);
			});
			Receive<AgentToLBSResponse>(delegate
			{
				Log.Information("Connected to LBServer.");
				ServerStatus.LBServerConnected = true;
				_lbserver.Tell(new ReloadHash
				{
					Hash = ServerSettingHolder.HashList.ToArray()
				});
			});
			Receive(delegate(AccountCheckRequest req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req, base.Sender);
				}
				else
				{
					IEnumerable<KeyValuePair<int, Account>> enumerable = ClientConnection.CurrentAccounts.Where((KeyValuePair<int, Account> p) => p.Value.UserID.ToLower() == req.UID.ToLower() && p.Value.isLogin);
					bool flag;
					if (req.ServerID == ServerStatus.MyAgentID)
					{
						flag = req.UID != string.Empty && enumerable.Count() > 1;
					}
					else
					{
						flag = req.UID != string.Empty && enumerable.Count() > 0;
						if (flag)
						{
							_lbserver.Tell(new AccountCheckResponse
							{
								Session = req.Session,
								ServerID = req.ServerID
							});
						}
					}
					if (flag)
					{
						if (ClientConnection.CurrentAccounts.TryGetValue(req.Session, out var value6))
						{
							value6.isBlocked = true;
							Log.Information("User [{0}] has already logged in! IP:{1}", value6.UserID, value6.LastIp);
							value6.Connection.SendAsync(new LoginError(6, 1, 0));
						}
						foreach (KeyValuePair<int, Account> item in enumerable)
						{
							if (item.Key != req.Session)
							{
								Log.Information("User [{0}] has already logged in! IP:{1}", item.Value.UserID, item.Value.LastIp);
								item.Value.Connection.SendAsync(new DisconnectPacket(259, 1));
								item.Value.Connection.Disconnect();
							}
						}
					}
				}
			});
			Receive(delegate(AccountCheckResponse req)
			{
				if (req.ServerID == ServerStatus.MyAgentID && ClientConnection.CurrentAccounts.TryGetValue(req.Session, out var value5))
				{
					value5.isBlocked = true;
					Log.Information("User [{0}] has already logged in! IP:{1}", value5.UserID, value5.LastIp);
					value5.Connection.SendAsync(new LoginError(6, 1, 0));
				}
			});
			Receive(delegate(NetPacket np)
			{
				_lbserver.Tell(np.ToArray());
			});
			Receive(delegate(ReloadSetting re)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					ReloadHandle(re);
				}
				else
				{
					_lbserver.Tell(re);
				}
			});
			Receive(delegate(CapsuleMachineItemUpdate re)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					if (CapsuleMachineHolder.CapsuleMachineContainer.TryGetValue(re.MachineNum, out var value4))
					{
						value4.ItemList.FirstOrDefault((CapsuleMachineItemNew w) => w.ItemNum == re.ItemNum).ItemCount--;
						if (re.isReset)
						{
							int currentRotateNum = value4.UpdateCapsuleMachineInfo();
							if (re.isRotate)
							{
								value4.isRotate = false;
								CapsuleMachineHolder.CurrentRotateNum = currentRotateNum;
								string machineItemNum = currentRotateNum.ToString();
								ServerStatus.ServerActor.Tell(new RoatateMachineNotice(machineItemNum));
							}
						}
					}
				}
				else
				{
					_lbserver.Tell(re);
				}
			});
			Receive(delegate(HuMongPickBoardUpdate re)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					if (EventPickBoardHolder.HuMongPickBoardContainer.TryGetValue(re.PickBoardNum, out var value3))
					{
						value3.ItemList.FirstOrDefault((HuMongPickBoardItemInfo w) => w.ItemNum == re.ItemNum).ItemCount--;
						value3.PickInfo[re.PickID] = true;
						if (re.isReset)
						{
							value3.UpdateHuMongPickBoardInfo();
						}
					}
				}
				else
				{
					_lbserver.Tell(re);
				}
			});
			Receive(delegate(byte[] np)
			{
				ServerStatus.ServerActor.Tell(np);
			});
			Receive(delegate(SetServerReady set)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					ServerStatus.form1.chkServerReady.Checked = set.isSet;
				}
				else
				{
					_lbserver.Tell(set);
				}
			});
			Receive(delegate(CanShopOperation set)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					ServerStatus.form1.chkOpenShop.Checked = set.isSet;
				}
				else
				{
					_lbserver.Tell(set);
				}
			});
			Receive(delegate(ReloadHash re)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					string[] hash = re.Hash;
					ServerSettingHolder.HashList = new HashSet<string>(hash);
					File.WriteAllLines("hash.ini", hash);
					Log.Information("Reload Hash Done!");
				}
				else
				{
					_lbserver.Tell(re);
				}
			});
			Receive(delegate(OnlineUserUpdate re)
			{
				_lbserver.Tell(re);
			});
			Receive(delegate(TotalLoginUser re)
			{
				ClientConnection.TotalAgentLoginUser = re.Count;
			});
			Receive(delegate(WaitLoginUserInfo re)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(re);
				}
				else if (re.Info.Item4 != ServerStatus.MyAgentID)
				{
					LoginTrafficManager.insertWaitLoginUserInfo(new ReLoginUserInfo
					{
						strID = re.Info.Item1,
						strIP = re.Info.Item2,
						Session = re.Info.Item3,
						iServerNum = re.Info.Item4
					});
				}
			});
			Receive(delegate(DelWaitLoginUserInfo re)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(re);
				}
				else
				{
					LoginTrafficManager.Logout(re.Info.Item2, re.Info.Item1, bSendToAllAgentServer: false);
				}
			});
			Receive(delegate(Guild_ProcessJoinRequest req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					Account value2 = ClientConnection.CurrentAccounts.FirstOrDefault((KeyValuePair<int, Account> f) => f.Value.NickName == req.NickName).Value;
					if (value2 != null)
					{
						short type = (short)(req.Accept ? 16 : 17);
						if (req.Accept)
						{
							GuildHandle.GetGuildInfo(value2.NickName, out var info2);
							value2.Connection.SendAsync(new GuildProcessJoinRequestACK2(type, req.guildName, req.guildNum, 1));
							value2.GuildNum = info2.guildNum;
							value2.GuildInfo = info2;
							value2.ItemNeedUpdateFromDB = true;
							NormalRoom room2 = Rooms.GetRoom(value2.CurrentRoomId);
							if (value2.InGame && room2 != null)
							{
								ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(value2, value2.NickName, info2.guildName, info2.guildNum, 0, -1, 1, 1), room2.RoomServerID);
							}
							if (GuildHandle.GetGuildFarmInfo(value2.GuildNum, value2.UserNum, out var farminfo))
							{
								value2.Connection.SendAsync(new GuildFarmInfoACK(farminfo, 1));
							}
						}
						else
						{
							value2.Connection.SendAsync(new GuildProcessJoinRequestACK2(type, req.guildName, req.guildNum, 1));
						}
					}
				}
			});
			Receive(delegate(Guild_ProcessLeave req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					Account value = ClientConnection.CurrentAccounts.FirstOrDefault((KeyValuePair<int, Account> f) => f.Value.NickName == req.NickName).Value;
					if (value != null)
					{
						value.GuildNum = -1;
						value.ItemNeedUpdateFromDB = true;
						if (req.bySelf == 0)
						{
							value.Connection.SendAsync(new GuildMemberUpdateACK(18, req.guildName, -1, 1));
						}
						NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
						if (value.InGame && room != null)
						{
							ServerStatus.ToRoomServer(new RM_GameRoomUpdateGuild(value, value.NickName, string.Empty, 0, -1, -1, 0, 1), room.RoomServerID);
						}
						value.Connection.SendAsync(new eServer_GET_LADDER_INFO_ACK(1));
						value.Connection.SendAsync(new GuildMissionUserMissionDeleteNotify_ACK(1, 1));
						value.Connection.SendAsync(new RemoveChallengingMission_ACK(2, 0, 1));
					}
				}
			});
			Receive(delegate(Guild_LVUP req)
			{
				GuildInfo info;
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else if (GuildHandle.GetGuildInfo(req.NickName, out info))
				{
					foreach (Account item2 in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin && w.GuildNum == info.guildNum))
					{
						item2.GuildInfo = info;
						item2.Connection.SendAsync(new Guild_LevelUP_ACK(info, 1));
					}
					if (info.level >= 5)
					{
						ServerStatus.ServerActor.Tell(new CommandHandle.ShoutToAll(info.guildName, 6, info.level, "..", 13111499991L, 1));
					}
				}
			});
			Receive(delegate(Guild_Skill req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					if (req.Type == 1)
					{
						foreach (Account item3 in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin && w.GuildNum == req.guildNum))
						{
							item3.ItemNeedUpdateFromDB = true;
						}
						return;
					}
					if (req.Type == 2)
					{
						foreach (Account item4 in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin && w.GuildNum == req.guildNum))
						{
							item4.ItemNeedUpdateFromDB = true;
							foreach (int item5 in req.items)
							{
								item4.activeItem.deleteItem(item5);
							}
						}
					}
				}
			});
			Receive(delegate(DisconnectUser req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					ClientConnection.CurrentAccounts.Values.FirstOrDefault((Account p) => p.NickName == req.NickName)?.Connection.Disconnect();
				}
			});
			Receive(delegate(HTLoad req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else if (req.type == 1)
				{
					HotTimeHolder.LoadHotTimeInfo();
					ServerStatus.ServerActor.Tell(new HotTimeInfos(1));
				}
				else if (req.type == 2)
				{
					HotTimeHolder.HotTimeInfos.Remove(req.hottimeid);
					ServerStatus.ServerActor.Tell(new HotTimeInfos(1));
				}
			});
			Receive(delegate(ParkDivinationCouple req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					ServerStatus.ServerActor.Tell(req);
				}
			});
			Receive(delegate(ThankOfferingScheduleReload req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					ServerSettingHolder.ServerSettings.ThankOfferingSchedule_OpenClose = "Open";
					ServerSettingHolder.ServerSettings.ThankOfferingSchedule_CurNum = req.ScheduleNum;
					ServerStatus.QuartzActor.Tell(new CreateJob(ThankOfferingSystem.ThankOfferingEndActor, req.ScheduleNum, TriggerBuilder.Create().StartAt(req.EndTime).WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
					{
						x.WithMisfireHandlingInstructionFireNow();
					})
						.Build()));
					ServerStatus.ServerActor.Tell(new ServerSettingChange("ThankOfferingSchedule_OpenClose", "Open", 1));
					ServerStatus.ServerActor.Tell(new ThankOffering_OnOff_ACK(isopen: true, 1));
					ServerStatus.ServerActor.Tell(new ServerSettingChange("ThankOfferingSchedule_CurNum", req.ScheduleNum.ToString(), 1));
				}
			});
			Receive(delegate(ThankOfferingScheduleNext req)
			{
				if (base.Sender.Path.ToString() != LBSPath)
				{
					_lbserver.Tell(req);
				}
				else
				{
					ServerStatus.QuartzActor.Tell(new CreateJob(ThankOfferingSystem.ThankOfferingStartActor, req.nextScheduleNum, TriggerBuilder.Create().StartAt(req.nextStartTime).WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
					{
						x.WithMisfireHandlingInstructionFireNow();
					})
						.Build()));
					ServerStatus.ServerActor.Tell(new ServerSettingChange("ThankOfferingSchedule_Reward", "Close", 1));
					ServerStatus.ServerActor.Tell(new ServerSettingChange("ThankOfferingSchedule_OpenClose", "Close", 1));
					ServerStatus.ServerActor.Tell(new ThankOffering_OnOff_ACK(isopen: false, 1));
					ServerStatus.ServerActor.Tell(new ServerSettingChange("ThankOfferingSchedule_Reward", "Open", 1));
				}
			});
			Receive(delegate(CombinationShopExchange re)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					CombinationShopHolder.setExchangeCount(re.SystemType, re.ExchangeID, re.LimitCount, re.ExchangeCount);
				}
				else
				{
					_lbserver.Tell(re);
				}
			});
			Receive(delegate(WeddingDivorce req)
			{
				if (base.Sender.Path.ToString() == LBSPath)
				{
					Account account = ClientConnection.CurrentAccounts.Values.FirstOrDefault((Account f) => f.NickName == req.NickName && f.isLogin);
					if (account != null)
					{
						if (req.DivorceType == 1 || req.DivorceType == 2)
						{
							account.resetCouple();
						}
						if (req.DivorceType == 0)
						{
							account.setCoupleType(req.DivorceType);
						}
					}
				}
				else
				{
					_lbserver.Tell(req);
				}
			});
		}

		private void ReloadHandle(ReloadSetting re)
		{
			switch (re.Code)
			{
			case 2:
				ReloadRenewalShop();
				break;
			case 3:
				ReloadHuMongPickBoard();
				break;
			case 4:
				ReloadCapsuleMachine();
				break;
			case 7:
				FishingHolder.LoadFishingInfo();
				break;
			case 8:
				ServerSettingHolder.LoadServerSettingInfo();
				ServerStatus.MainActorSystem.ActorSelection("/user/Client/*").Tell(new NP_Byte(DBInit.GameServerSetting));
				break;
			case 5:
			case 6:
				break;
			}
		}

		private void ReloadRenewalShop()
		{
			ShopHolder.LoadNewShopInfo();
			Task.Run(delegate
			{
				foreach (Account item in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin))
				{
					item.Connection.SendAsync(new GetShopCategoryList(16));
					item.Connection.SendAsync(new GetShopDisplayItemList(16));
					item.Connection.SendAsync(new GetShopItemSellList(16));
					item.Connection.SendAsync(new GetShopDisplayDateLimitList(16));
					item.Connection.SendAsync(new GetShopBuyLimitCountList(16));
					item.Connection.SendAsync(new GetShopBuyAddBenefitList(16));
				}
			});
			Log.Information("Reload RenewalShopInfo Done!");
		}

		private void ReloadHuMongPickBoard()
		{
			EventPickBoardHolder.LoadHuMongPickBoardInfo();
		}

		private void ReloadCapsuleMachine()
		{
			CapsuleMachineHolder.LoadCapsuleMachineInfo();
		}
	}
}
