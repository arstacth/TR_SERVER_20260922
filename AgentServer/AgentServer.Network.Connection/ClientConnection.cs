using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AgentServer.EasyAntiCheat;
using AgentServer.Function;
using AgentServer.Holders;
using AgentServer.Network;
using AgentServer.Packet;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Mission;
using Akka.Actor;
using Akka.IO;
using LocalCommons.Cryptography;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using NetMsg.Room;
using Serilog;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;

namespace AgentServer.Network.Connections
{
	public class ClientConnection : ReceiveActor
	{
		public class Net_OnConnection : NetPacket
		{
			public Net_OnConnection()
			{
				ns.Write((short)14);
				ns.Write((short)1);
			}
		}

		private readonly IActorRef _connection;

		private readonly object DDOS_Lock = new object();

		public static int TotalAgentLoginUser { get; set; } = 0;


		public static ConcurrentDictionary<int, Account> CurrentAccounts { get; } = new ConcurrentDictionary<int, Account>();


		public static ConcurrentDictionary<string, ConnectionInfo> DDOS_IP { get; } = new ConcurrentDictionary<string, ConnectionInfo>();


		public EndPoint EP { get; set; }

		public string IP { get; private set; }

		public Account CurrentAccount { get; set; }

		public int session { get; set; }

		public string authStatus { get; set; } = string.Empty;


		private void test()
		{
			test();
		}

		public ClientConnection(IActorRef client, EndPoint remote)
		{
			ClientConnection clientConnection = this;
			UntypedActor.Context.Watch(client);
			_connection = client;
			EP = remote;
			IP = ((IPEndPoint)EP).Address.ToString();
			Log.Information("Client: {0} connected", IP);
			DDOS_Filter(IP, 1);
			session = Session.Generate(ServerStatus.MyAgentID);
			Account account2 = (CurrentAccount = new Account
			{
				Connection = this,
				Session = session,
				bLogin = false,
				LoginAuthPassed = false
			});
			ServerStatus.LBServerActor.Tell(new OnlineUserUpdate
			{
				OnlineCount = CurrentAccounts.Count,
				LoginedCount = CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin)
			});
			MemoryStream memoryStream = new MemoryStream();
			Receive(delegate(Tcp.Received received)
			{
				int count = received.Data.Count;
				memoryStream.Write(received.Data.ToArray(), 0, count);
				byte[] array = memoryStream.ToArray();
				int num = 0;
				if (count > 0)
				{
					while (true)
					{
						int num2 = 0;
						num2 = ((array.Length - num >= 9) ? BitConverter.ToInt32(array, num) : (-1));
						if (array.Length - num < num2 || num2 == -1)
						{
							break;
						}
						BitConverter.ToUInt32(array, num + 4);
						int num3 = num2 - 9;
						byte[] array2 = new byte[num3];
						Buffer.BlockCopy(array, num + 8, array2, 0, num3);
						clientConnection.HandleReceived(array2);
						num += num2;
					}
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = new MemoryStream();
					memoryStream.Write(array, num, array.Length - num);
				}
			});
			Receive(delegate(UserEnterRoomOK rsp)
			{
				clientConnection.CurrentAccount.CurrentRoomId = rsp.RoomID;
				clientConnection.CurrentAccount.InGame = true;
				clientConnection.CurrentAccount.RoomPos = rsp.Pos;
				clientConnection.CurrentAccount.RoomServerID = rsp.RoomServerID;
			});
			Receive<UserLeaveRoomOK>(delegate
			{
				clientConnection.CurrentAccount.CurrentRoomId = 0;
				clientConnection.CurrentAccount.InGame = false;
				clientConnection.CurrentAccount.RoomPos = 0;
				clientConnection.CurrentAccount.RoomServerID = 0;
			});
			Receive(delegate(UpdateItemInfo rsp)
			{
				clientConnection.CurrentAccount.activeItem.updateItemCount(rsp.ItemNum, rsp.Count);
			});
			Receive(delegate(UpdateUserInfo rsp)
			{
				int level = clientConnection.CurrentAccount.Level;
				clientConnection.CurrentAccount.Exp = rsp.EXP;
				clientConnection.CurrentAccount.TR = rsp.GameMoney;
				LobbyHandle.LevelUPCheck(clientConnection.CurrentAccount, level);
			});
			Receive<ReloadDailyMission>(delegate
			{
				if (clientConnection.CurrentAccount.LoginDateTime < MissionHolder.EndReloadTime)
				{
					clientConnection.CurrentAccount.DailyMissionStartTime = DateTime.Now;
					clientConnection.SendAsync(new GetUserDailyMission_ACK(clientConnection.CurrentAccount, 1));
					clientConnection.SendAsync(new OneDayMissionReload_ACK(clientConnection.CurrentAccount, 1));
				}
			});
			Receive(delegate(ParkDivinationCouple rsp)
			{
				if (rsp.CoupleNum == clientConnection.CurrentAccount.CoupleInfo.CoupleNum && rsp.UserNum != clientConnection.CurrentAccount.UserNum)
				{
					ParkHandle.divinationUpdateCoupleAbility(clientConnection.CurrentAccount, rsp.ItemNum);
				}
			});
			Receive(delegate(NetPacket packet)
			{
				clientConnection.SendAsync(packet);
			});
			Receive(delegate(byte[] packet)
			{
				clientConnection.SendAsync(packet);
			});
			Receive<Tcp.ConnectionClosed>(delegate
			{
				try
				{
					clientConnection.ClientConnection_DisconnectedEvent();
					Log.Information("Client: {0} disconnected", remote);
				}
				catch (Exception)
				{
					Log.Warning("Client: {0} disconnected,But the remove fail", remote);
				}
				UntypedActor.Context.Stop(clientConnection.Self);
			});
			Receive<Terminated>(delegate
			{
				try
				{
					clientConnection.ClientConnection_DisconnectedEvent();
					Log.Information("Client: {0} died", remote);
				}
				catch
				{
					Log.Warning("Client: {0} die,But the remove fail", remote);
				}
				UntypedActor.Context.Stop(clientConnection.Self);
			});
		}

		public void SendAsync(NetPacket packet)
		{
			try
			{
				if (packet != null && !CurrentAccount.isDisconnected && packet.ToArray().Length >= 2)
				{
					_connection.Tell(Tcp.Write.Create(ByteString.FromBytes(EncryptPacket(packet.ToArray()))));
				}
			}
			catch (Exception ex)
			{
				Log.Error("ClientConnection SendAsync Error:{0} isLogin:{1}", ex.ToString(), CurrentAccount.isLogin);
			}
		}

		public void SendAsync(byte[] packet)
		{
			try
			{
				if (packet != null && packet.Length >= 2 && !CurrentAccount.isDisconnected)
				{
					byte[] array = EncryptPacket(packet);
					_connection.Tell(Tcp.Write.Create(ByteString.FromBytes(array)));
				}
			}
			catch (Exception ex)
			{
				Log.Error("ClientConnection SendAsync byte[] Error:{0} isLogin:{1}", ex.ToString(), CurrentAccount.isLogin);
			}
		}

		private byte[] EncryptPacket(byte[] packet)
		{
			if (packet != null && packet.Length >= 2)
			{
				ushort wireOp = (ushort)(packet[0] | (packet[1] << 8));
				ProtocolDump.Tx(wireOp, wireOp, packet, CurrentAccount != null && CurrentAccount.isLogin);
			}
			bool encrypt = CurrentAccount.bLogin && CurrentAccount.EncryptKey != null;
			if (encrypt)
			{
				// newEncryptByte: AES full blocks + XOR remainder + Concat(1).
				// Client decrypt window is frame_len-10 (strips Concat). Plaintext
				// len%16==15 (1481/737/1059) is valid — do NOT zero-pad (Remain=1).
				// Do NOT drop Concat — that desyncs every packet / blocks enter.
				packet = Encrypt.newEncryptByte(CurrentAccount.EncryptKey, CurrentAccount.XorKey, packet);
			}
			int total = packet.Length + (encrypt ? 9 : 8);
			byte[] result = new byte[total];
			Buffer.BlockCopy(BitConverter.GetBytes(total), 0, result, 0, 4);
			Buffer.BlockCopy(packet, 0, result, 8, packet.Length);
			if (encrypt)
			{
				result[total - 1] = 1;
			}
			byte[] crcSrc = new byte[encrypt ? packet.Length + 1 : packet.Length];
			Buffer.BlockCopy(packet, 0, crcSrc, 0, packet.Length);
			if (encrypt)
			{
				crcSrc[packet.Length] = 1;
			}
			Buffer.BlockCopy(BitConverter.GetBytes(Utility.CheckSum(crcSrc)), 0, result, 4, 4);
			return result;
		}

		public void ClientConnection_DisconnectedEvent()
		{
			try
			{
				EACServer.OnLeaveGame(session);
				LoginTrafficManager.Logout(session, CurrentAccount.UserID, bSendToAllAgentServer: true);
				CurrentAccount.isDisconnected = true;
				CurrentAccounts.TryRemove(session, out var _);
				ServerStatus.LBServerActor.Tell(new OnlineUserUpdate
				{
					OnlineCount = CurrentAccounts.Count,
					LoginedCount = CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin)
				});
				if (CurrentAccount.isLogin && !CurrentAccount.isBlocked)
				{
					CurrentAccount.isFishing = false;
					if (CurrentAccount.FishingCancelSource != null)
					{
						CurrentAccount.FishingCancelSource.Cancel();
					}
					MissionUpdate();
					ServerStatus.ToAllRoomServer(new RM_PlayerLeaveRoom(CurrentAccount, isDisconnect: true, 1));
					CurrentAccount.DisconnectedEvent();
					if (Partys.GetParty(CurrentAccount.CurrentPartyID, out var party))
					{
						party.LeaveParty(CurrentAccount, 0, 1);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (CurrentAccount.isLogin && !CurrentAccount.isBlocked)
				{
					HandleLogout(CurrentAccount);
					CurrentAccount.bLogin = false;
				}
			}
		}

		public void Disconnect()
		{
			_connection.Tell(Tcp.Close.Instance);
		}

		public async void Disconnect(int delay)
		{
			CurrentAccount.isDisconnected = true;
			await Task.Delay(delay);
			_connection.Tell(Tcp.Close.Instance);
		}

		private void PackedLogout(byte last, string reason)
		{
			Log.Information("Packed logout via {0} user={1}", reason, CurrentAccount != null ? CurrentAccount.UserID : "?");
			SendAsync(new PackedDisconnectFromServerAck(last));
			if (CurrentAccount != null && CurrentAccount.isLogin && !CurrentAccount.isBlocked)
			{
				HandleLogout(CurrentAccount);
				CurrentAccount.bLogin = false;
			}
			Disconnect(1500);
		}

		private void HandleLogout(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_logout";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
				mySqlCommand.Parameters.Add("puid", MySqlDbType.VarString).Value = User.UserID;
				mySqlCommand.Parameters.Add("pexp", MySqlDbType.Int64).Value = User.Exp;
				mySqlCommand.Parameters.Add("ip", MySqlDbType.VarString).Value = User.LastIp;
				mySqlCommand.Parameters.Add("logintime", MySqlDbType.DateTime).Value = User.LoginDateTime;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("Logout sql error: {0}", ex.Message);
			}
		}

		private void MissionUpdate()
		{
			if (MissionHolder.MissionReloading || DateTime.Now.Date != MissionHolder.EndReloadTime.Date || !CurrentAccount.DailyMission.MissionInfo.Any((KeyValuePair<int, MissionInfo> a) => a.Value.challengeState < 2))
			{
				return;
			}
			string text = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			foreach (KeyValuePair<int, MissionInfo> item in from w in CurrentAccount.DailyMission.MissionInfo
				where w.Value.challengeState == 0
				select w into o
				orderby o.Key
				select o)
			{
				foreach (KeyValuePair<int, MissionConditionInfo> item2 in item.Value.ConditionInfo)
				{
					if (MissionHolder.ConditionInfoByConditionNum.TryGetValue(item2.Key, out var value) && value.type == 71)
					{
						int achievedPoint = CurrentAccount.DailyMission.MissionInfo[item.Key].ConditionInfo[item2.Key].achievedPoint;
						int num = (int)(DateTime.Now - CurrentAccount.DailyMissionStartTime).TotalSeconds + achievedPoint;
						text += $"{item.Key},";
						text2 += $"{item2.Key},";
						text3 += $"{num},";
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Mission.mission_ConditionUpdateAchievedPoints(CurrentAccount.UserNum, text, text2, text3, out var _);
			}
		}

		private void HandleReceived(byte[] data)
		{
			if (!ServerStatus.isReady || CurrentAccount == null || CurrentAccount.isDisconnected)
			{
				return;
			}
			PacketReader packetReader = new PacketReader(data, 0);
			ushort num = 0;
			try
			{
				if (CurrentAccount.isLogin)
				{
					packetReader.Decrypt(CurrentAccount.EncryptKey, CurrentAccount.XorKey);
				}
				num = packetReader.ReadLEUInt16();
				ushort wire = num;
				ProtocolDump.Rx(wire, num, packetReader.Buffer, CurrentAccount.isLogin, IP);
				if (Conf.ProtocolDebug && wire != 322 && wire != 1254)
				{
					Log.Warning("RX wire {0} server {1} bytes {2} login:{3} ip:{4}", wire, num, data.Length, CurrentAccount.isLogin, IP);
				}
				if (Conf.ProtocolDebug && (wire == 1 || num == 1))
				{
					Log.Warning("LOGIN_REQ hex {0}", BitConverter.ToString(data, 0, Math.Min(data.Length, 96)));
				}
				if (num >= 8192)
				{
					Log.Error("[PACKET]invalid protocol ({0}) IP: {1}", num, IP);
					DDOS_Filter(IP, 2);
					CurrentAccount.isDisconnected = true;
					Disconnect();
					return;
				}
				if (!CurrentAccount.isLogin && num != (ushort)Opcodes.eServer_LOGIN_AUTH_REQ && num != (ushort)Opcodes.eServer_ECC_SESSIONKEY_EXCHANGE_REQ && num != (ushort)Opcodes.eServer_PRELOGIN_19 && num != (ushort)Opcodes.eServer_LOGIN_REQ && num != (ushort)Opcodes.eServer_NOTIFY_MY_UDP_REQ && num != (ushort)Opcodes.eServer_I_AM_COMPLETE_FULL_CONNECTION && num != (ushort)Opcodes.eServer_LIVE_MSG && num != (ushort)Opcodes.eServer_DOM_USER_INFO_ACK && num != (ushort)Opcodes.eServer_GET_SERVER_TIME_REQ && num != (ushort)Opcodes.eServer_RX_1077 && num != (ushort)Opcodes.eServer_PRELOGIN_1188)
				{
					Log.Warning("Pre-login opcode {0} bytes {1} ip:{2}", num, data.Length, IP);
					return;
				}
				byte b = packetReader.Buffer.LastOrDefault();
				if (CurrentAccount.GotClientKey && CurrentAccount.isLogin)
				{
					if (CurrentAccount.SequenceNum == 0)
					{
						CurrentAccount.SequenceNum = (byte)((b > 128) ? 1 : (b = (byte)(b * 2)));
					}
					else
					{
						if (CurrentAccount.SequenceNum != b)
						{
							Log.Warning("[{0}] error in sequence num({3})({1}), ip:{2}", CurrentAccount.NickName, b, IP, num);
							return;
						}
						CurrentAccount.SequenceNum = (byte)((b > 128) ? 1 : (b = (byte)(b * 2)));
					}
				}
				switch ((Opcodes)num)
			{
				case Opcodes.eServer_CLIENT_WEB_PAGE_URL_ACK:
					if (!ServerStatus.CanShopOperation)
					{
						SendAsync(new ShopClosed(b));
						return;
					}
					break;
				
			}
				switch ((Opcodes)num)
			{
				case Opcodes.eServer_LOGIN_AUTH_REQ:
					LoginHandle.Handle_LoginCheck(this, packetReader);
					break;
				case Opcodes.eServer_ECC_SESSIONKEY_EXCHANGE_REQ:
					if (Conf.ProtocolDebug)
					{
						Log.Information("GetClientKey opcode {0} bytes {1}", num, data.Length);
					}
					LoginHandle.Handle_GetClientKey(this, packetReader);
					break;
				case Opcodes.eServer_LOGIN_REQ:
					if (CurrentAccount == null || !CurrentAccount.LoginAuthPassed)
					{
						Log.Warning("LOGIN_REQ rejected: login auth not passed ip:{0}", IP);
						SendAsync(new LoginError(13, b));
						return;
					}
					CurrentAccount.bLogin = true;
					LoginHandle.Handle_LoginSuccess(this, packetReader, b);
					break;
				case Opcodes.eServer_NOTIFY_MY_UDP_REQ:
					LoginHandle.Handle_NOTIFY_MY_UDP(this, packetReader, b);
					break;
				case Opcodes.eServer_GetGameOption:
					LobbyHandle.Handle_GetGameOption(this, b);
					break;
				case Opcodes.eServer_GAME_OPTION_SET_REQ:
					// Thai wire 1651 (large option dump / set). Was UNHANDLED → wrong TR look until re-equip.
					LobbyHandle.Handle_SetGameOption(this, packetReader, b);
					break;
				case Opcodes.eServer_I_AM_COMPLETE_FULL_CONNECTION:
					if (CurrentAccount == null || !CurrentAccount.LoginAuthPassed)
					{
						Log.Warning("I_AM_COMPLETE rejected: login auth not passed ip:{0}", IP);
						return;
					}
					if (Conf.ProtocolDebug)
					{
						Log.Information("I_AM_COMPLETE_FULL_CONNECTION user={0}", CurrentAccount.UserID);
					}
					MissionHolder.optional_collectionMission_Load(CurrentAccount);
					// LOGIN_OK TR is often ignored until HUD esc/shop — push CURRENT_TR at enter.
					ShopHandle.RefreshGameMoney(CurrentAccount);
					SendAsync(new CurrentGameMoney_ACK(CurrentAccount, b));
					break;
				case Opcodes.eServer_Blocking_To_Me_REQ:
					GMCommandHandle.Handle_ClientCheckAutoBan(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_NICKNAME_REQ:
					LoginHandle.Handle_GetNickName(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_EXP_REQ:
					LobbyHandle.Handle_eServer_GET_EXP_REQ(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ITEM_COLLECTION_INFO_REQ:
					LobbyHandle.Handle_eServer_GET_ITEM_COLLECTION_INFO_REQ(this, packetReader, b);
					break;
				case Opcodes.eServer_ItemCollectionProtocol:
					LobbyHandle.Handle_ItemCollectionProtocol(this, packetReader, b);
					break;
				case Opcodes.eServer_CapsuleMachineV2Protocol:
					ParkHandle.Handle_CapsuleMachineV2Protocol(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_COLLECTION_USER_LIST_REQ:
					LobbyHandle.Handle_eServer_ITEM_COLLECTION_USER_LIST_REQ(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_COLLECTION_ADD_REQ:
					LobbyHandle.Handle_eServer_ITEM_COLLECTION_ADD_REQ(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_HOTTIME_INFO_REQ:
					HotTimeHandle.Handle_eServer_GET_HOTTIME_INFO_REQ(this, b);
					break;
				case Opcodes.eServer_ITEM_ONOFF_REQ:
					MyRoomHandle.Handle_ItemOnOff(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_NICKNAME_ENTER_MY_NICKNAME_REQ:
					FirstLoginHandle.Handle_SetNewNickName(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ITEMMSG_REQ:
					MyRoomHandle.Handle_ItemMsgPop(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_REQ:
					ItemHandle.Handle_GetActiveFuncItem(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_IN_POSITION_REQ:
					ItemHandle.Handle_GetActiveFuncItem_Position(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_LIST_REQ:
					ItemHandle.Handle_GetActiveFuncItem_List(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_AVATAR_REQ:
					ItemHandle.Handle_GetCurrentAvatarInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_ANIMAL_AVATAR_REQ:
					LoginHandle.Handle_82(this, b);
					break;
				case Opcodes.eServer_SELECT_START_CHARACTER_REQ:
					FirstLoginHandle.Handle_SelectStartCharacter(this, packetReader, b);
					break;
				case Opcodes.eServer_CLIENT_WEB_PAGE_URL_REQ:
					LobbyHandle.Handle_ShowPage(this, packetReader, b);
					break;
				case Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL:
				{
					short num4 = packetReader.ReadLEInt16();
					switch ((eCommunityProtocol)num4)
					{
					case eCommunityProtocol.GET_COMMUNITY_AGENT_SERVER_REQ:
						LoginHandle.Handle_GetCommunityAgentServer(this, b);
						break;
					case eCommunityProtocol.ADD_FRIEND_REQ:
						CommunityHandle.Handle_AddFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.GET_FRIEND_LIST_ACCEPTED_REQ:
						CommunityHandle.Handle_GetFriendListAccepted(this, b);
						break;
					case eCommunityProtocol.ACCEPT_FRIEND_REQ:
						CommunityHandle.Handle_AcceptFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.DECLINE_FRIEND_REQ:
						CommunityHandle.Handle_DeclineFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.BLOCK_FRIEND_REQ:
						CommunityHandle.Handle_BlockFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.CHECK_GIFT_REQ:
						CommunityHandle.Handle_CheckGift(this, b);
						break;
					case eCommunityProtocol.UNBLOCK_FRIEND_REQ:
						CommunityHandle.Handle_UnBlockFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.DELETE_FRIEND_REQ:
						CommunityHandle.Handle_DeleteFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.CANCEL_ADD_FRIEND_REQ:
						CommunityHandle.Handle_CancelAddFriend(this, packetReader, b);
						break;
					case eCommunityProtocol.GET_REQUESTED_TO_ME_REQ:
						CommunityHandle.Handle_GetRequestedToMe(this, b);
						break;
					case eCommunityProtocol.GET_FRIEND_GROUP_REQ:
						CommunityHandle.Handle_GetFriendGroup(this, packetReader, b);
						break;
					case eCommunityProtocol.GROUP_MOVE_MEMBER_REQ:
						CommunityHandle.Handle_GroupMoveMember(this, packetReader, b);
						break;
					case eCommunityProtocol.COMMUNITY_38_REQ:
						CommunityHandle.Handle_0x7426(this, b);
						break;
					case eCommunityProtocol.GET_GUILD_MEMBER_LIST_REQ:
						CommunityHandle.Handle_GetGuildMemberList(this, packetReader, b);
						break;
					case eCommunityProtocol.UPDATE_GUILD_MEMBER_LIST_REQ:
						CommunityHandle.Handle_UpdateGuildMemberList(this, b);
						break;
					case eCommunityProtocol.MODIFY_MEMO_REQ:
						CommunityHandle.Handle_ModifyMemo(this, packetReader, b);
						break;
					case eCommunityProtocol.COMMUNITY_46_REQ:
						CommunityHandle.Handle_Community46(this, b);
						break;
					default:
						if (Conf.ProtocolDebug)
						{
							Log.Warning("Unhandled community subopcode {0}", num4);
						}
						break;
					}
					break;
				}
				case Opcodes.eServer_GET_SERVER_TIME_REQ:
					LobbyHandle.HandlePingTime(this, 1, b);
					break;
				case Opcodes.eServer_LIVE_MSG:
					if (CurrentAccount != null)
					{
						CurrentAccount.LastPingTime = Utility.CurrentTimeMilliseconds();
					}
					break;
				case Opcodes.eServer_USE_SHOUT_ITEM_REQ:
					CommandHandle.Handle_UseShoutItem(this, packetReader, b);
					break;
				case Opcodes.eServer_HOTTIME_EVENT_SETTING_REQ:
					HotTimeHandle.Handle_SetHotTimeInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_HOTTIME_EVENT_DELETE_REQ:
					HotTimeHandle.Handle_DeleteHotTimeEvent(this, packetReader, b);
					break;
				case Opcodes.eServer_HOTTIME_EVENT_APPLY_REQ:
					HotTimeHandle.Handle_ApplyHotTimeEvent(this, packetReader, b);
					break;
				case Opcodes.eServer_RANK_BY_NUM_REQ:
					RankHandle.Handle_GetRankInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_RANK_BY_SEARCH_REQ:
					RankHandle.Handle_SearchRank(this, packetReader, b);
					break;
				case Opcodes.eServer_RANK_MY_NICKNAME_REQ:
					RankHandle.Handle_GetMyRankInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_USER_INFO_REQ:
					LobbyHandle.Handle_GetUserInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_USER_INFO_OPTION_SET_REQ:
					LobbyHandle.Handle_SetGameOption(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_USER_ALARM_LIST_REQ:
					CommunityHandle.Handle_GetUserAlarmInfo(this, b);
					break;
				case Opcodes.eServer_GET_AVATAR_ITEMS_REQ:
					ItemHandle.Handle_GetAvatarItems(this, packetReader, b);
					break;
				case Opcodes.eServer_CHECK_JUDGEMENT_REQ:
					LoginHandle.Handle_FF7F01(this, b);
					break;
				case Opcodes.eServer_ReportSystemUserInfo:
					LoginHandle.Handle_ReportSystemUserInfo(this, b);
					break;
				case Opcodes.eServer_PieroPostboxUserInfo:
					LoginHandle.Handle_PieroPostboxUserInfo(this, b);
					break;
				case Opcodes.eServer_ReportSystemUserReportInfo:
					LoginHandle.Handle_ReportSystemUserReportInfo(this, b);
					break;
				case Opcodes.eServer_MESSAGE_KEEP_ACK:
					UnknownHandle.Handle_FF9701(this, b);
					break;
				case Opcodes.eServer_SHOP_REQUEST_CHECK_CASH_REQ:
					LoginHandle.Handle_GetUserCash(this, b);
					break;
				case Opcodes.eServer_SHOP_CATEGORY_REQ:
					ShopHandle.Handle_GetShopCategoryList(this, b);
					break;
				case Opcodes.eServer_SHOP_DISPLAY_LIST_REQ:
					ShopHandle.Handle_GetShopDisplayList(this, b);
					break;
				case Opcodes.eServer_SHOP_USER_VIP_LEVEL_REQ:
					ShopHandle.Handle_GetUserVip(this, b);
					break;
				case Opcodes.eServer_SHOP_USER_BUY_COUNT_LIST_REQ:
					ShopHandle.Handle_GetUserBuyList(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_BUY_COUNT_LIST_REQ:
					ShopHandle.Handle_GetShopPurchasingLimitList(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_RANK_REQ:
					ShopHandle.Handle_GetShopCategoryDisplayItem(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_USER_WISH_LIST_REQ:
					UnknownHandle.Handle_FFCF01(this, b);
					break;
				case Opcodes.eServer_IS_EXIST_CONFIRMATION_PASSWORD_REQ:
					UnknownHandle.Handle_FFD501(this, b);
					break;
				case Opcodes.eServer_EXTRA_ABILITY_LIST_REQ:
					LoginHandle.Handle_GetExtraAbilities(this, b);
					break;
				case Opcodes.eServer_MYROOM_REQ:
					switch ((eMyRoomProtocol)packetReader.ReadLEInt16())
					{
					case eMyRoomProtocol.eServer_MYROOM_TRANSACTION_REQ:
						MyRoomHandle.Handle_FFCF0100(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_GET_MY_CHARACTER_LIST_REQ:
						MyRoomHandle.Handle_MyRoomGetCharacterList(this, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_GET_MY_CARDS_LIST_REQ:
						MyRoomHandle.Handle_MyRoomGetMyCards(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_GET_MY_CHARACTER_SETTING_REQ:
						MyRoomHandle.Handle_MyRoomGetCharacterSetting(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_SAVE_CHARACTER_SETTING_REQ:
						MyRoomHandle.Handle_SaveCharSetting(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_RESET_REQ:
						MyRoomHandle.Handle_CharacterStatReset(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_CONFIRM_REQ:
						MyRoomHandle.Handle_CharacterStatConfirm(this, packetReader, b);
						break;
					case eMyRoomProtocol.eServer_MYROOM_SAVE_DEFAULT_CHARACTER_REQ:
						MyRoomHandle.Handle_SaveDefaultCharacter(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_LUCKY_BAG_REQ:
						MyRoomHandle.Handle_UseLuckyBag(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_PET_FEED_REQ:
						MyRoomHandle.Handle_FeedPet(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_PET_REBIRTH_REQ:
						MyRoomHandle.Handle_PetRebirth(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_PET_UPGRADE_REQ:
						MyRoomHandle.Handle_PetUpgrade(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_REPAIR_ITEM_REQ:
						MyRoomHandle.Handle_RepairItem(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_USE_COUPLE_EXP_ADD_ITEM_REQ:
						CoupleHandle.Handle_UseCoupleExpAddItem(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_SET_SLOTITEM_SETTING_REQ:
						MyRoomHandle.Handle_SetSlotItemSetting(this, packetReader, b);
						break;
					case eMyRoomProtocol._WIRE_244:
						MyRoomHandle.Handle_SetSlotItemSetting(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_GET_USERSLOT_INFO_REQ:
						MyRoomHandle.Handle_GetUserSlotInfo(this, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_LIST_FAVORITES_REQ:
						MyRoomHandle.Handle_GetFavoriteList(this, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_ADD_FAVORITES_REQ:
						MyRoomHandle.Handle_AddFavorite(this, packetReader, b);
						break;
					case eMyRoomProtocol.eMyRoomProtocol_REMOVE_FAVORITES_REQ:
						MyRoomHandle.Handle_RemoveFavorite(this, packetReader, b);
						break;
					}
					break;
				case Opcodes.eServer_FARM_REQ:
					switch ((FarmProtocol)packetReader.ReadLEInt16())
					{
					case FarmProtocol.EnterFarm_REQ:
						FarmHandle.Handle_EnterFarm(this, packetReader, b);
						break;
					case FarmProtocol.CreatePublicFarm_REQ:
						FarmHandle.Handle_CreatePublicFarm(this, packetReader, b);
						break;
					case FarmProtocol.GetFarmPoint_REQ:
						FarmHandle.Handle_GetFarmPoint(this, b);
						break;
					case FarmProtocol.PublicFarmList_REQ:
					case FarmProtocol.PieroFarmList_REQ:
						FarmHandle.Handle_GetPublicFarmList(this, packetReader, b);
						break;
					case FarmProtocol.GetMyFarmInfo_REQ:
						SendAsync(new LoginFarm_GetMyFarmInfo(CurrentAccount, b));
						break;
					case FarmProtocol.EnterGuildFarm_REQ:
						GuildFarmHandle.Handle_EnterGuildFarm(this, packetReader, b);
						break;
					case FarmProtocol.GetGuildFarmInfo_REQ:
						GuildFarmHandle.Handle_GetGuildFarmInfo(this, packetReader, b);
						break;
					case FarmProtocol.GetGuildFarmObjectAttr_REQ:
						GuildFarmHandle.Handle_GetGuildFarmObjectAttr(this, packetReader, b);
						break;
					case FarmProtocol.ModifyGuildFarmNoticeBoardInfo_REQ:
						GuildFarmHandle.Handle_ModifyGuildFarmNoticeBoardInfo(this, packetReader, b);
						break;
					case FarmProtocol.GetGuildFarmItemList_REQ:
						GuildFarmHandle.Handle_GetGuildFarmItemList(this, packetReader, b);
						break;
					case FarmProtocol.GetMyFarmItemList_REQ:
						FarmHandle.Handle_GetMyFarmItem(this, packetReader, b);
						break;
					case FarmProtocol.GetFarmItemList_REQ:
						FarmHandle.Handle_GetFarmItemList(this, packetReader, b);
						break;
					case FarmProtocol.FarmItemAttr_REQ:
						FarmHandle.Handle_GetFarmItemAttr(this, packetReader, b);
						break;
					case FarmProtocol.ReloadFarmMapInfo_REQ:
					case FarmProtocol._WIRE_573:
						FarmHandle.Handle_ReloadFarmMapInfo(this, packetReader, b);
						break;
					case FarmProtocol._WIRE_1522:
						FarmHandle.Handle_FFD10136(this, packetReader, b);
						break;
					case FarmProtocol.ClearUserFarmMapInfo_REQ:
						FarmHandle.Handle_ClearUserFarmMapInfo(this, packetReader, b);
						break;
					case FarmProtocol.ModifyFarmMapInfo_REQ:
						FarmHandle.Handle_ModifyFarmMapInfo(this, packetReader, b);
						break;
					case FarmProtocol._WIRE_1226:
						FarmHandle.Handle_SearchFarm(this, packetReader, b);
						break;
					case FarmProtocol.SearchFarmByUserNum_REQ:
						FarmHandle.Handle_SearchFarmByUserNum(this, packetReader, b);
						break;
					case FarmProtocol.JoinFarmRoom_REQ:
						FarmHandle.Handle_JoinFarmRoom(this, packetReader, b);
						break;
					case FarmProtocol._WIRE_168:
						FarmHandle.Handle_IncreaseAnimalSize(this, packetReader, b);
						break;
					case FarmProtocol.RestoreAnimalDefaultSize_REQ:
						FarmHandle.Handle_RestoreAnimalDefaultSize(this, packetReader, b);
						break;
					case FarmProtocol.ChangeFarmType_REQ:
						FarmHandle.Handle_ChangeFarmType(this, packetReader, b);
						break;
					case FarmProtocol.ModifyObjectValueInfo_REQ:
						FarmHandle.Handle_ModifyObjectValueInfo(this, packetReader, b);
						break;
					case FarmProtocol.Expired_farm_item_REQ:
						FarmHandle.Handle_ExpiredFarmItem(this, packetReader, b);
						break;
					case FarmProtocol.ChangeFarmSkybox_REQ:
						FarmHandle.Handle_ChangeFarmSkybox(this, packetReader, b);
						break;
					case FarmProtocol.ChangeFarmWeather_REQ:
						FarmHandle.Handle_ChangeFarmWeather(this, packetReader, b);
						break;
					case FarmProtocol.GetMasterUserInfo_REQ:
					case FarmProtocol.GetMasterUserInfo_REQ_KR:
						FarmHandle.Handle_GetMasterUserInfo(this, packetReader, b);
						break;
					case FarmProtocol.FarmProtocol_UNK2_REQ:
						FarmHandle.Handle_FFD1017E(this, b);
						break;
					case FarmProtocol._WIRE_126:
						FarmHandle.Handle_FFD1017E(this, b);
						break;
					case FarmProtocol.SetFarmPortalInfo_REQ:
						FarmHandle.Handle_SetFarmPortalInfo(this, packetReader, b);
						break;
					case FarmProtocol._WIRE_883:
						FarmHandle.Handle_GetFarmPortalInfo(this, packetReader, b);
						break;
					case FarmProtocol.GetFarmSlotListInfo_REQ:
						FarmHandle.Handle_GetFarmSlotListInfo(this, b);
						break;
					case FarmProtocol._WIRE_1067:
						FarmHandle.Handle_ClearFarmSlotInfo(this, packetReader, b);
						break;
					case FarmProtocol.ChangeFarmMapTypeBySlot_REQ:
						FarmHandle.Handle_ChangeFarmMapTypeBySlot(this, packetReader, b);
						break;
					default:
						Log.Warning("Unhandled farm nested {0} remain={1} user:{2}", packetReader.Offset > 2 ? (int)packetReader.Buffer[packetReader.Offset - 2] | (packetReader.Buffer[packetReader.Offset - 1] << 8) : -1, packetReader.Remaining, CurrentAccount != null ? CurrentAccount.UserID : "?");
						break;
					}
					break;
				case Opcodes.eServer_ALONERUN_START_GAME_REQ:
					LobbyHandle.Handle_SinglePlay(this, packetReader, b);
					break;
				case Opcodes.eServer_ALONERUN_GOAL_IN_REQ:
					LobbyHandle.Handle_SinglePlayGoalResult(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_USER_MISSION_LIST_REQ:
					Mission.Handle_GetUserMissionInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_ONEDAY_MISSION_GET_USER_ONEDAY_MISSION_REQ:
					Mission.Handle_GetUserDailyMissionInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_ONEDAY_MISSION_GET_USER_MISSION_FINISH_COUNT_REQ:
					Mission.Handle_GetDailyMissionFinishedInfo(this, b);
					break;
				case Opcodes.eServer_ONEDAY_MISSION_GET_EVENT_MISSION_STATUS_REQ:
					UnknownHandle.Handle_FF5602(this, b);
					break;
				case Opcodes.eServer_OPTIONAL_MISSION_GET_USER_INFO_REQ:
					Mission.Handle_UserOptionalMissionInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COLLECTION_MISSION_GET_USER_MISSION_REQ:
					Mission.Handle_CollectionMissionInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_USER_MISSION_ADD_REQ:
					Mission.Handle_AddChallengingMission(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_USER_MISSION_REMOVE_REQ:
					Mission.Handle_RemoveChallengingMission(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_USER_MISSION_UPDATE_REQ:
					Mission.Handle_UpdateMission(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_USER_MISSION_COMPLETE_CHECK_REQ:
					Mission.Handle_CompleteMission(this, packetReader, b);
					break;
				case Opcodes.eServer_MISSION_GIVE_REWARD_REQ:
					Mission.Handle_MissionGiveReward(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILD_MISSION_GET_USER_GUILD_MISSION_REQ:
					Mission.Handle_GetGuildMissionList(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILD_MISSION_SET_MASTER_MISSION_REQ:
					Mission.Handle_SetGuildMasterMission(this, packetReader, b);
					break;
				case Opcodes.eServer_EMBLEM_ACQUIRE_REQ:
					Mission.Handle_AcquireEmblem(this, packetReader, b);
					break;
				case Opcodes.eServer_QUEST_ADD_REQ:
					Mission.Handle_QuestAdd(this, packetReader, b);
					break;
				case Opcodes.eServer_QUEST_REMOVE_REQ:
					Mission.Handle_QuestRemove(this, packetReader, b);
					break;
				case Opcodes.eServer_QUEST_REWARD_REQ:
					Mission.Handle_QuestReward(this, packetReader, b);
					break;
				case Opcodes.eServer_EMBLEM_EVENT_LIST_REQ:
					UnknownHandle.Handle_FF6602(this, b);
					break;
				case Opcodes.eServer_EMBLEM_USER_INFO_LIST_REQ:
					UnknownHandle.Handle_FF6802(this, b);
					break;
				case Opcodes.eServer_FARM_HARVEST_ITEM_EXCHANGE_REWARD_REQ:
					FarmHandle.Handle_FarmExchangeItem(this, packetReader, b);
					break;
				case Opcodes.eServer_STANDBY_PURCHASE_DICISION_GET_LIST_REQ:
					UnknownHandle.Handle_FFA905(this, b);
					break;
				case Opcodes.eServer_SHU_PROTOCOL:
				{
					byte b3 = packetReader.ReadByte();
					packetReader.Offset--;
					switch ((eShuProtocol)b3)
					{
					case eShuProtocol.HATCH_REQ:
						ShuSystemHandle.Handle_Shu_Hatch(this, packetReader, b);
						break;
					case eShuProtocol.GET_ITEM_INFO_BY_STR_REQ:
						ShuSystemHandle.Handle_Shu_GetItemInfoByStr(this, packetReader, b);
						break;
					case eShuProtocol.GET_USER_ITEM_INFO_REQ:
						ShuSystemHandle.Handle_Shu_GetUserItemInfo(this, packetReader, b);
						break;
					case eShuProtocol.MANAGER_ACTION_REQ:
						ShuSystemHandle.Handle_Shu_ManagerAction(this, packetReader, b);
						break;
					case eShuProtocol.CHANGE_NAME_REQ:
						ShuSystemHandle.Handle_Shu_ChangeName(this, packetReader, b);
						break;
					case eShuProtocol.CHANGE_AVATAR_INFO_REQ:
						ShuSystemHandle.Handle_Shu_ChangeAvatarInfo(this, packetReader, b);
						break;
					case eShuProtocol.CHANGE_CURRENT_SHU_REQ:
						ShuSystemHandle.Handle_Shu_ChangeCurrentShu(this, packetReader, b);
						break;
					case eShuProtocol.USE_ITEM_REQ:
						ShuSystemHandle.Handle_Shu_UseItem(this, packetReader, b);
						break;
					case eShuProtocol.GET_GIFT_REQ:
						ShuSystemHandle.Handle_Shu_GetGift(this, packetReader, b);
						break;
					case eShuProtocol.EXPLORE_CHECK_REQ:
						ShuSystemHandle.Handle_Shu_ExploreCheck(this, packetReader, b);
						break;
					case eShuProtocol.EXPLORE_START_REQ:
						ShuSystemHandle.Handle_Shu_ExploreStart(this, packetReader, b);
						break;
					case eShuProtocol.EXPLORE_STOP_REQ:
						ShuSystemHandle.Handle_Shu_ExploreStop(this, packetReader, b);
						break;
					case eShuProtocol.EXPLORE_REWARD_REQ:
						ShuSystemHandle.Handle_Shu_ExploreReward(this, packetReader, b);
						break;
					}
					break;
				}
				case Opcodes.eServer_USERPOINT_VER2_REQ:
					LobbyHandle.Handle_GetUserPoint(this, packetReader, b);
					break;
				case Opcodes.eServer_AVATAR_LOCK_SAVE_REQ:
					AvatarLock.Handle_AvatarLock_Save(this, packetReader, b);
					break;
				case Opcodes.eServer_AVATAR_LOCK_LOAD_REQ:
					AvatarLock.Handle_AvatarLock_Load(this, packetReader, b);
					break;
				case Opcodes.eServer_MAKE_ROOM_REQ:
					GameRoomHandle.Handle_CreateGameRoom(this, packetReader, b);
					break;
				case Opcodes.eServer_LEAVE_ROOM_REQ:
					GameRoomHandle.Handle_LeaveRoom(this, packetReader, b);
					break;
				case Opcodes.eServer_NUMBER_FOR_PREVENT_ABUSING_REQ:
					GameRoomHandle.Handle_GetVertification(this, b);
					break;
				case Opcodes.eServer_CONFIRM_NUMBER_FOR_PREVENT_ABUSING_REQ:
					GameRoomHandle.Handle_PassVertification(this, packetReader, b);
					break;
				case Opcodes.eServer_ROOM_KIND_ATTR_REQ:
					GameRoomHandle.Handle_GetRoomKindAttr(this, packetReader, b);
					break;
				case Opcodes.eServer_ROOM_LIST_REQ:
					GameRoomHandle.Handle_GetRoomList(this, packetReader, b);
					break;
				case Opcodes.eServer_ENTER_ROOM_REQ:
					GameRoomHandle.Handle_EnterRoom(this, packetReader, b);
					break;
				case Opcodes.eServer_QUICK_JOIN_REQ:
				case Opcodes.eServer_QUICK_JOIN_EX_REQ:
					GameRoomHandle.Handle_RandomEnterRoom(this, packetReader, b);
					break;
				case Opcodes.eServer_ENTER_ROOM_EX_REQ:
					GameRoomHandle.Handle_PlayTogether(this, packetReader, b);
					break;
				case Opcodes.eServer_RoomControl:
					GameRoomHandle.Handle_RoomControl(this, packetReader, b);
					break;
				case Opcodes.eServer_PackedRoomWrap:
					GameRoomHandle.Handle_PackedRoomWrap(this, packetReader, b);
					break;
				case Opcodes.eServer_KICK_PLAYER_REQ:
					GameRoomHandle.Handle_KickPlayer(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_BUY_ASSISTITEM_TOGETHER_REQ:
					GameRoomHandle.Handle_PlayerList(this, packetReader, b);
					break;
				case Opcodes.eServer_RACE_LENGTH_REQ:
					GameRoomHandle.Handle_GameEndInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_OPEN_SELECTIVE_PACKAGE_REQ:
					ShopHandle.Handle_OpenSelectivePackage(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_BUY_PRODUCTS_TOGETHER_REQ:
					ShopHandle.Handle_BuyItem(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_GIFT_PRODUCTS_REQ:
					ShopHandle.Handle_GiftItem(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_AVATAR_ITEM_ONE_REQ:
					ItemHandle.Handle_GetAvatarItemOne(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_AVATAR_ITEM_LIST_REQ:
					ItemHandle.Handle_GetAvatarItemList(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_CURRENT_TR:
					ShopHandle.Handle_GetCurrentGameMoney(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_GIFT_ACCEPT_WAIT_LIST_REQ:
					MyRoomHandle.Handle_GetGiftList(this, packetReader, b);
					break;
				case Opcodes.eServer_SHOP_ACCEPT_GIFT_REQ:
					MyRoomHandle.Handle_AcceptGift(this, packetReader, b);
					break;
				case Opcodes.eServer_PERMANENCE_ITEM_REQ:
					ItemHandle.Handle_PermanenceItem(this, packetReader, b);
					break;
				case Opcodes.eServer_GET_USER_ITEM_ATTR_REQ:
					MyRoomHandle.Handle_MyroomGetUserItemAttr(this, packetReader, b);
					break;
				case Opcodes.eServer_CAPSULE_MACHINE_INFO__VER2_REQ:
					ParkHandle.Handle_GetMachineInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_PAY_CASH_REQ:
					ParkHandle.Handle_Alchemist_MachineSelect(this, packetReader, b);
					break;
				case Opcodes.eServer_CAPSULE_MACHINE_GIVE_REQ:
					ParkHandle.Handle_MachineReceiveORGiftItem(this, packetReader, b);
					break;
				case Opcodes.eServer_STORAGE_SAVE_ITEM_REQ:
					ParkHandle.Handle_MachineKeepItem(this, packetReader, b);
					break;
				case Opcodes.eServer_STORAGE_ITEM_LIST_REQ:
					MyRoomHandle.Handle_GetStorageItemList(this, packetReader, b);
					break;
				case Opcodes.eServer_STORAGE_GIFT_ITEM_REQ:
					MyRoomHandle.Handle_StorageItemGift(this, packetReader, b);
					break;
				case Opcodes.eServer_STORAGE_RECEIVE_ITEM_REQ:
					MyRoomHandle.Handle_StorageItemReceive(this, packetReader, b);
					break;
				case Opcodes.eServer_RECEIVE_ITEM_REQ:
				case Opcodes.eServer_RECEIVE_ITEM_REQ2:
					ParkHandle.Handle_ReceiveItem(this, packetReader, b);
					break;
				case Opcodes.eServer_EXCHANGE_SYSTEM_GET_USE_INFO_REQ:
					ExchangeHandle.Handle_GetExchangeSystemInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_EXCHANGE_SYSTEM_EXCHANGE_REQ:
					ExchangeHandle.Handle_ExchangeItem(this, packetReader, b);
					break;
				case Opcodes.eServer_COMBINATION_SHOP_GET_USE_INFO_REQ:
					CombinationShopHandle.Handle_GetUseInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COMBINATION_SHOP_GET_LIMIT_COUNT_INFO_REQ:
					CombinationShopHandle.Handle_GetLimitCountInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COMBINATION_SHOP_EXCHANGE_REQ:
					CombinationShopHandle.Handle_ShopExchange(this, packetReader, b);
					break;
				case Opcodes.eServer_COMBINATION_SHOP_GET_ITEM_DETAIL_INFO_REQ:
					CombinationShopHandle.Handle_ItemDetailInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_NOTICE_MSG_REQ:
					GMCommandHandle.Handle_Notice(this, packetReader, b);
					break;
				case Opcodes.eServer_DISCONNECT_USER_REQ:
					GMCommandHandle.Handle_DisconnectUser(this, packetReader, b);
					break;
				case Opcodes.eServer_DISCONNECT_IF_NOT_GM_USER_REQ:
					GMCommandHandle.Handle_FindGo(this, packetReader, b);
					break;
				case Opcodes.eServer_REPORT_BAD_USER_REQ:
					GMCommandHandle.Handle_BlockUser(this, packetReader);
					break;
				case Opcodes.eServer_EVENT_PICK_BOARD_USER_INFO_REQ:
					EventPickBoardHandle.Handle_GetEventPickBoardInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_EVENT_PICK_BOARD_USE_REQ:
					EventPickBoardHandle.Handle_EventPickBoard_Use(this, packetReader, b);
					break;
				case Opcodes.eServer_EVENT_PICK_BOARD_GIVE_REQ:
					EventPickBoardHandle.Handle_EventPickBoard_Give(this, packetReader, b);
					break;
				case Opcodes.eServer_ENCHANT_SYSTEM_GET_MY_ITEM_REQ:
					EnchantSystem.Handle_GetEnchantItemInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_ENCHANT_SYSTEM_MOUNT_REQ:
					EnchantSystem.Handle_StoneMount(this, packetReader, b);
					break;
				case Opcodes.eServer_ENCHANT_SYSTEM_REMOVE_REQ:
					EnchantSystem.Handle_StoneRemove(this, packetReader, b);
					break;
				case Opcodes.eServer_ENCHANT_SYSTEM_REMOVE_SEAL_REQ:
					EnchantSystem.Handle_SealErase(this, packetReader, b);
					break;
				case Opcodes.eServer_ENCHANT_SYSTEM_STONE_HARDENING_REQ:
					EnchantSystem.Handle_Hardening(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_TRADING_CHECK:
					ItemTradeHandle.Handle_ItemTrading_Check(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_TRADING_TRADE:
					ItemTradeHandle.Handle_ItemTrading_Trade(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_TRADING_COMPLETE:
					ItemTradeHandle.Handle_ItemTrading_Complete(this, packetReader, b);
					break;
				case Opcodes.eServer_MOTION_QUICKSLOT_UPDATE_REQ:
					LobbyHandle.Handle_SetHotKey(this, packetReader, b);
					break;
				case Opcodes.eServer_MOTION_QUICKSLOT_INFO_REQ:
					LobbyHandle.Handle_GetHotKey(this, b);
					break;
				case Opcodes.eServer_ANUBIS_EXPEDITION_GET_INFO_REQ:
					AssaultModeHandle.Handle_GetAnubisOpenTime(this, b);
					break;
				case Opcodes.eServer_ANUBIS_EXPEDITION_GET_USER_INFO_REQ:
					AssaultModeHandle.Handle_GetAnubisPoint(this, b);
					break;
				case Opcodes.eServer_CARDPACK_OPENCARDPACK_REQ:
					LobbyHandle.Handle_CardPackOpen(this, packetReader, b);
					break;
				case Opcodes.eServer_DUNGEON_RAID_SCHEDULE_INFO_REQ:
					AssaultModeHandle.Handle_GetAssaultRaidOpenTime(this, b);
					break;
				case Opcodes.eServer_DUNGEON_RAID_GET_MY_POINT_REQ:
					AssaultModeHandle.Handle_GetDungeonRaidPoint(this, b);
					break;
				case Opcodes.eServer_ROOMKIND_ENTRY_CONDITION_REQ:
					AssaultModeHandle.Handle_GetAssaultModeLimitAttackInfo(this, b);
					break;
				case Opcodes.eServer_GET_USER_INFO_REQ__CHALLENGE_FULLRECORD:
					SingleChallengeHandle.Handle_GetUserInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_CHALLENGE_MAP_START_REQ:
					SingleChallengeHandle.Handle_StartChallenge(this, packetReader, b);
					break;
				case Opcodes.eServer_CHALLENGE_MAP_END_REQ:
					SingleChallengeHandle.Handle_ChallengeAction(this, packetReader, b);
					break;
				case Opcodes.eServer_CHALLENGE_MAP_SUMMARY_REQ:
					SingleChallengeHandle.Handle_MapSummary(this, packetReader, b);
					break;
				case Opcodes.eServer_CHALLENGE_MAP_NPCMATCH_MAP_INFO_REQ:
					SendAsync(new ChallengeMapNPCMatchMapInfoAck(b));
					break;
				case Opcodes.eServer_ITEMCUBE_CUBEINFO_REQ:
					CubeHandle.Handle_CubeCheck(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEMCUBE_CUBEOPEN_REQ:
					CubeHandle.Handle_CubeOpen(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEMCUBE_ACCEPT_REQ:
					CubeHandle.Handle_CubeItemAccept(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_DYEING_DYE_ITEM_REQ:
					DyeingHandle.Handle_ItemDyeing(this, packetReader, b);
					break;
				case Opcodes.eServer_ITEM_DYEING_DECOLOR_ITEM_REQ:
					DyeingHandle.Handle_ItemDyeingRestore(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_SEND_REQ:
					MessageBoxHandle.Handle_SendMessage(this, packetReader, b);
					break;
				case Opcodes.eServer_SEARCH_NICKNAME_REQ:
					MessageBoxHandle.Handle_SearchNickName(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_BOX_LIST_REQ:
					MessageBoxHandle.Handle_GetReceiveList(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_READ_REQ:
					MessageBoxHandle.Handle_ReadMessage(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_ACCUSE_REQ:
					MessageBoxHandle.Handle_ReportMessage(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_DELETE_REQ:
					MessageBoxHandle.Handle_DeleteMessage(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_KEEP_REQ:
					MessageBoxHandle.Handle_KeepMessage(this, packetReader, b);
					break;
				case Opcodes.eServer_MESSAGE_GET_OPTION_REQ:
					MessageBoxHandle.Handle_GetOption(this, b);
					break;
				case Opcodes.eServer_MESSAGE_CHANGE_OPTION_REQ:
					MessageBoxHandle.Handle_OptionChange(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_CHECK_PROPOSE_INFO_REQ:
					CoupleHandle.Handle_CheckProposeInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_CHANGE_COUPLE_RING_REQ:
					CoupleHandle.Handle_ChangeCoupleRing(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_INIT_RECV_PROPOSE_INFO_REQ:
					CoupleHandle.Handle_InitProposeInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_CREATE_COUPLE_INFO_REQ:
					CoupleHandle.Handle_CreateCoupleInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_MODIFY_COUPLE_INFO_REQ:
					CoupleHandle.Handle_ModifyCoupleInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_MODIFY_COUPLE_NAME_REQ:
					CoupleHandle.Handle_ModifyCoupleName(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_REMOVE_COUPLE_INFO_REQ:
					CoupleHandle.Handle_RemoveCoupleInfo(this, b);
					break;
				case Opcodes.eServer_COUPLE_GET_USER_COUPLE_INFO_REQ:
					CoupleHandle.Handle_GetCoupleInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COUPLE_UPDATE_USER_COUPLE_INFO_REQ:
					CoupleHandle.Handle_UpdateCoupleInfo(this, b);
					break;
				case Opcodes.eServer_WEDDING_SUIT_FOR_DIVORCE_REQ:
					CoupleHandle.Handle_WeddingSuitForDivorce(this, packetReader, b);
					break;
				case Opcodes.eServer_WEDDING_INIT_DIVORCE_REQUEST_INFO_REQ:
					CoupleHandle.Handle_WeddingDivorceReject(this, packetReader, b);
					break;
				case Opcodes.eServer_WEDDING_DIVORCE_REQ:
					CoupleHandle.Handle_WeddingDivorce(this, packetReader, b);
					break;
				case Opcodes.eServer_FAMILY_GET_FAMILY_INFO_REQ:
					CoupleHandle.Handle_GetFamilyInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_FAMILY_CHECK_PROPOSE_CONDITION_REQ:
					CoupleHandle.Handle_MakeFamilyCheck(this, packetReader, b);
					break;
				case Opcodes.eServer_FAMILY_MAKE_FAMILY_REQ:
					CoupleHandle.Handle_MakeFamily(this, packetReader, b);
					break;
				case Opcodes.eServer_FAMILY_DISSOLVE_FAMILY_REQ:
					CoupleHandle.Handle_DissolveFamily(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_MYUNITINFO_REQ:
					TalesKnightHandle.Handle_GetMyTalesKnightUnitInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_MYGROUPINFO_REQ_NAME:
					TalesKnightHandle.Handle_GetMyTalesKnightsGroupName(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_MYGROUPINFO_REQ_UNIT:
					TalesKnightHandle.Handle_GetMyTalesKnightsGroupInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGNT_MYGROUP_UNIT_UPDATE_REQ:
					TalesKnightHandle.Handle_UpdateTalesKnightsGroup(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGNT_MYGROUP_NAME_UPDATE_REQ:
					TalesKnightHandle.Handle_UpdateTalesKnightsGroup_Name(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_UNITCHECK_REQ:
					TalesKnightHandle.Handle_HasTalesKnightUnit(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_STAGEINFO_REQ:
					TalesKnightHandle.Handle_GetTalesKnightStageInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_ENTER_ADVENTURE_REQ:
					TalesKnightHandle.Handle_TalesKnightStageEnter(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_ADVENTURE_RESULT_REQ:
					TalesKnightHandle.Handle_TalesKnights_AttackCheck(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_PVE_REWARD_RECEIVE_REQ:
					TalesKnightHandle.Handle_TalesKnightsRewardReceive(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_PVE_COMEBACK_REQ:
					TalesKnightHandle.Handle_MyKnightsCallComeBack(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_REINFORCE_UNIT_REQ:
					TalesKnightHandle.Handle_TalesKnightsAdd_MaxLevel(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_USE_EXPUP_ITEM_REQ:
					TalesKnightHandle.Handle_TalesKnightsUseExpUpItem(this, packetReader, b);
					break;
				case Opcodes.eServer_TALESKNIGHT_DUELINFO_PVP_REQ:
					UnknownHandle.Handle_FF8406(this, b);
					break;
				case Opcodes.eServer_FISHING_MY_PICTURE_BOOK_REQ:
					FishingHandle.Handle_GetFishRecordInfo(this, b);
					break;
				case Opcodes.eServer_FISHING_MY_KEEP_NET_REQ:
					FishingHandle.Handle_GetFishNetInfo(this, b);
					break;
				case Opcodes.eServer_FISHING_PROC_FISHING_REQ:
					FishingHandle.Handle_Fishing(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_RECEIVE_FROM_KEEP_NET_REQ:
					FishingHandle.Handle_CollectFishedItem(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_FARM_MASTER_REWARD_REQ:
					FishingHandle.Handle_GetFarmFishingReward(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_REGIST_FARM_MASTER_REWARD_REQ:
					FishingHandle.Handle_SetFarmFishingReward(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_REMOVE_FARM_MASTER_REWARD_REQ:
					FishingHandle.Handle_RemoveFarmFishingReward(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_FINISH_MINIGAME_REQ:
					FishingHandle.Handle_MiniGameFishing(this, packetReader, b);
					break;
				case Opcodes.eServer_HUMONG_PICKBOARD_STATE_REQ:
					EventPickBoardHandle.Handle_GetHuMongPickBoardInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_HUMONG_PICKBOARD_PICK_REQ:
					EventPickBoardHandle.Handle_HuMongPickBoard_PickItem(this, packetReader, b);
					break;
				case Opcodes.eServer_HUMONG_PICKBOARD_CONFIRM_REQ:
					EventPickBoardHandle.Handle_HuMongPickBoard_GiveItem(this, packetReader, b);
					break;
				case Opcodes.eServer_PARTY_SYSTEM_PROTOCOL:
				{
					packetReader.Offset += 4;
					byte b2 = packetReader.ReadByte();
					packetReader.Offset += 3;
					switch ((ePartyProtocol)b2)
					{
					case ePartyProtocol.INVITE_REQ:
						PartyHandle.Handle_PartyInvite(this, packetReader, b);
						break;
					case ePartyProtocol.ACCEPT_INVITE_REQ:
						PartyHandle.Handle_AcceptPartyInvite(this, packetReader, b);
						break;
					case ePartyProtocol.LEAVE_REQ:
						PartyHandle.Handle_PartyLeave(this, packetReader, b);
						break;
					case ePartyProtocol.KICK_REQ:
						PartyHandle.Handle_PartyKickUser(this, packetReader, b);
						break;
					case ePartyProtocol.CHANGE_LEADER_REQ:
						PartyHandle.Handle_PartyChangeLeader(this, packetReader, b);
						break;
					case ePartyProtocol.RECRUIT_REQ:
						PartyHandle.Handle_PartyRecruit(this, packetReader, b);
						break;
					case ePartyProtocol.RECRUIT_CANCEL_REQ:
						PartyHandle.Handle_PartyRecruitCancel(this, packetReader, b);
						break;
					case ePartyProtocol.GET_PARTY_INDEX_REQ:
						PartyHandle.Handle_GetPartyIndex(this, packetReader, b);
						break;
					case ePartyProtocol.GET_PARTY_USER_LIST_REQ:
						PartyHandle.Handle_GetPartyUserList(this, packetReader, b);
						break;
					case ePartyProtocol.JOIN_REQUEST_REQ:
						PartyHandle.Handle_PartyJoinRequest(this, packetReader, b);
						break;
					case ePartyProtocol.JOIN_REQUEST_REJECT_REQ:
						PartyHandle.Handle_PartyJoinRequestReject(this, packetReader, b);
						break;
					case ePartyProtocol.GET_JOIN_REQUEST_LIST_REQ:
						PartyHandle.Handle_GetPartyJoinRequestList(this, packetReader, b);
						break;
					}
					break;
				}
				case Opcodes.eServer_GUILD_OPERATION_REQ:
				{
					short num3 = packetReader.ReadLEInt16();
					switch ((eGuildProtocol)num3)
					{
					case eGuildProtocol.CHECK_GUILD_NAME_REQ:
						GuildHandle.Handle_CheckGuildName(this, packetReader, b);
						break;
					case eGuildProtocol.MAKE_GUILD_REQ:
						GuildHandle.Handle_MakeGuild(this, packetReader, b);
						break;
					case eGuildProtocol.DEL_GUILD_REQ:
						GuildHandle.Handle_DelGuild(this, packetReader, b);
						break;
					case eGuildProtocol.GET_GUILD_LIST_REQ:
						GuildHandle.Handle_GetGuildList(this, packetReader, b);
						break;
					case eGuildProtocol.REQUEST_JOIN_REQ:
						GuildHandle.Handle_RequestJoin(this, packetReader, b);
						break;
					case eGuildProtocol.CANCEL_PROPOSE_REQ:
						GuildHandle.Handle_CancelPropose(this, packetReader, b);
						break;
					case eGuildProtocol.PROCESS_JOIN_REQUEST_REQ:
						GuildHandle.Handle_ProcessJoinRequest(this, packetReader, b);
						break;
					case eGuildProtocol.PROCESS_LEAVE_REQ:
						GuildHandle.Handle_ProcessLeave(this, packetReader, b);
						break;
					case eGuildProtocol.GET_JOIN_REQUEST_LIST_REQ:
						GuildHandle.Handle_GetJoinRequestList(this, b);
						break;
					case eGuildProtocol.MODIFY_JOIN_LIMIT_LEVEL_REQ:
						GuildHandle.Handle_ModifyJoinLimitLevel(this, packetReader, b);
						break;
					case eGuildProtocol.MODIFY_JOIN_METHOD_REQ:
						GuildHandle.Handle_ModifyJoinMethod(this, packetReader, b);
						break;
					case eGuildProtocol.MODIFY_MEMBER_GRADE_REQ:
						GuildHandle.Handle_ModifyMemberGrade(this, packetReader, b);
						break;
					case eGuildProtocol.MODIFY_MESSAGE_REQ:
						GuildHandle.Handle_ModifyMessage(this, packetReader, b);
						break;
					case eGuildProtocol.LEVEL_UP_REQ:
						GuildHandle.Handle_LevelUP(this, b);
						break;
					case eGuildProtocol.GET_GUILD_INFO_REQ:
						GuildHandle.Handle_GetGuildInfo(this, packetReader, b);
						break;
					case eGuildProtocol.GET_CONTRIBUTION_POINT_REQ:
						GuildHandle.Handle_GetContributionPoint(this, packetReader, b);
						break;
					case eGuildProtocol.GET_GUILD_POINT_REQ:
						GuildHandle.Handle_GetGuildPoint(this, packetReader, b);
						break;
					case eGuildProtocol.USE_GIFT_BOX_REQ:
						GuildHandle.Handle_UseGiftBox(this, packetReader, b);
						break;
					case eGuildProtocol.GET_GUILD_USER_INFO_REQ:
						Console.Write(0);
						break;
					case eGuildProtocol.ADD_GUILD_SKILL_REQ:
						GuildHandle.Handle_AddGuildSkill(this, packetReader, b);
						break;
					case eGuildProtocol.RESET_GUILD_SKILL_REQ:
						GuildHandle.Handle_ResetGuildSkill(this, packetReader, b);
						break;
					default:
						Log.Debug("Unknown Guild subopcode: 0x{0:X2}", num3);
						break;
					}
					break;
				}
				case Opcodes.eServer_TUTORIAL_CHANNEL_USER_INFO_REQ:
					TutorialChannel.Handle_GetUserInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_TUTORIAL_CHANNEL_GIVE_REWARD_REQ:
					TutorialChannel.Handle_RequestReward(this, packetReader, b);
					break;
				case Opcodes.eServer_STRENGTHEN_REINFORCE_ITEM_REQ:
					ArinHandle.Handle_ItemStrengthen_StrengthenSlot(this, packetReader, b);
					break;
				case Opcodes.eServer_STRENGTHEN_PURIFY_ITEM_REQ:
					ArinHandle.Handle_ItemStrengthen_CleanSlot(this, packetReader, b);
					break;
				case Opcodes.eServer_STRENGTHEN_RECHARGE_REINFORCE_CHANCE_REQ:
					ArinHandle.Handle_IncreaseStrengthenCount(this, packetReader, b);
					break;
				case Opcodes.eServer_TRANSFORM_ITEM_REQ:
					ArinHandle.Handle_ItemTransform(this, packetReader, b);
					break;
				case Opcodes.eServer_ANNIVERSARY_OBJECT_ACTION_REQ:
					Anniversary.Handle_ObjectAction(this, packetReader, b);
					break;
				case Opcodes.eServer_ANNIVERSARY_GET_OBJECT_VALUE_REQ:
					Anniversary.Handle_GetObjectValue(this, packetReader, b);
					break;
				case Opcodes.eServer_ANNIVERSARY_GET_RECEIVED_REWARD_GRADE_LIST_REQ:
					Anniversary.Handle_GetReveivedRewardGradeList(this, packetReader, b);
					break;
				case Opcodes.eServer_ANNIVERSARY_RECEIVE_REWARD_REQ:
					Anniversary.Handle_ReceiveReward(this, packetReader, b);
					break;
				case Opcodes.eServer_ARCHIVES_GET_USERINFO_REQ:
					if (wire == 690)
					{
						CapybaraShopHandle.Handle_UserInfo(this, packetReader, b);
					}
					else
					{
						Archives.Handle_GetUserInfo(this, b);
					}
					break;
				case Opcodes.eServer_Trade:
					CapybaraShopHandle.Handle_Trade(this, packetReader, b);
					break;
				case Opcodes.eServer_TodaySchedule:
					CapybaraShopHandle.Handle_TodaySchedule(this, packetReader, b);
					break;
				case Opcodes.eServer_ARCHIVES_USER_REWARD_REQ:
					Archives.Handle_GetReward(this, packetReader, b);
					break;
				case Opcodes.eServer_ARCHIVE_ARTIFACT_EXCHANGE_REQ:
					Archives.Handle_Archives_Exchange(this, packetReader, b);
					break;
				case Opcodes.eServer_TALES_MARBLE_PROTOCOL:
					switch ((eTalesMarbleProtocol)packetReader.ReadLEInt32())
					{
					case eTalesMarbleProtocol.GET_USERINFO_AND_REWARD_REQ:
						DiceBoardHandle.Handle_GetDiceBoardUserInfoAndRewardInfo(this, packetReader, b);
						break;
					case eTalesMarbleProtocol.DRAW_REQ:
						DiceBoardHandle.Handle_DiceBoard_Draw(this, packetReader, b);
						break;
					case eTalesMarbleProtocol.FILL_GAUGE_REQ:
						DiceBoardHandle.Handle_DiceBoard_FillGauge(this, packetReader, b);
						break;
					case eTalesMarbleProtocol.GET_LIST_REQ:
						DiceBoardHandle.Handle_GetDiceBoardList(this, packetReader, b);
						break;
					case eTalesMarbleProtocol.GET_USERINFO_REQ:
						DiceBoardHandle.Handle_GetDiceBoardUserInfo(this, packetReader, b);
						break;
					case eTalesMarbleProtocol.RESET_REQ:
						DiceBoardHandle.Handle_DiceBoard_Reset(this, packetReader, b);
						break;
					}
					break;
				case Opcodes.eServer_THANK_OFFERING_PROTOCOL:
				{
					int num2 = packetReader.ReadLEInt32();
					packetReader.Offset += 4;
					switch ((eThankOfferingProtocol)num2)
					{
					case eThankOfferingProtocol.REWARD_REQ:
						ThankOfferingHandle.Handle_Reward(this, packetReader, b);
						break;
					case eThankOfferingProtocol.GET_USER_POINT_REQ:
						ThankOfferingHandle.Handle_GetUserPoint(this, packetReader, b);
						break;
					case eThankOfferingProtocol.GET_RANK_REQ:
						ThankOfferingHandle.Handle_GetRank(this, packetReader, b);
						break;
					}
					break;
				}
				case Opcodes.eServer_USE_EXTRA_ABILITY_ITEM_REQ:
					MyRoomHandle.Handle_UseExtraAbilityItem(this, packetReader, b);
					break;
				case Opcodes.eServer_Divination_CHECK_FREE_REQ:
					ParkHandle.Handle_CheckDivinationFree(this, packetReader, b);
					break;
				case Opcodes.eServer_Divination_GET_RESULT_FREE_REQ:
					ParkHandle.Handle_ParkUseDivinationFree(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILDMATCH_RANK_BY_NUM_REQ:
					GuildMatchHandle.Handle_GuildMatch_GetRankRange(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILDMATCH_GET_LATEST_MATCH_REQ:
					GuildMatchHandle.Handle_GuildMatch_GetRankMyGuild(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILDMATCH_GET_PARTY_ID_REQ:
					GameRoomHandle.Handle_JoinGuildMatch(this, b);
					break;
				case Opcodes.eServer_GUILDMATCH_OBSERVER_ENTER_ROOM_RESERVATION_REQ:
					GuildMatchHandle.Handle_LookingForGuildMatch(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILDMATCH_SEARCHING_PARTY_START_REQ:
					GuildMatchHandle.Handle_PickGuildForMatch(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILDMATCH_SEARCHING_PARTY_CANCEL:
					GuildMatchHandle.Handle_CancelLookingForGuildMatch(this, b);
					break;
				case Opcodes.eServer_GUILDMATCH_MATCH_ACTION_REQ:
					GuildMatchHandle.Handle_ChooseGuildForMatch(this, packetReader, b);
					break;
				case Opcodes.eServer_ENTER_ROOM_GUILDMATCH_REQ:
					GameRoomHandle.Handle_EnterRoomForGuild(this, packetReader, b);
					break;
				case Opcodes.eServer_CHANNEL_INFO_REQ:
					LobbyHandle.Handle_GetOfficialCompetitionOpenTime(this, packetReader, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_PARTY_USER_INFO_REQ:
					CompetitionEventHandle.Handle_PartyUserInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_PARTY_JOIN_REQ:
					CompetitionEventHandle.Handle_PartyJoin(this, packetReader, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_PARTY_INFO_REQ:
					CompetitionEventHandle.Handle_GetPartyPointInfo(this, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_POINT_REWARD_REQ:
					CompetitionEventHandle.Handle_PointReward(this, packetReader, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_TODAY_GAME_REQ:
					CompetitionEventHandle.Handle_TodayGameInfo(this, b);
					break;
				case Opcodes.eServer_FRONTIER_CHANNEL_SCHEDULE_INFO_REQ:
					CompetitionEventHandle.Handle_FronTier_SchduleInfo(this, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_PLAYERCOUNT_LIST_REQ:
					CompetitionEventHandle.Handle_GetRoomKindPlayerNum(this, b);
					break;
				case Opcodes.eServer_FISHING_MY_ASYNC_FISHING_POINT_REQ:
					FishingHandle.Handle_AsyncFishingPoint(this, b);
					break;
				case Opcodes.eServer_SEASON_CHANNEL_SCHEDULE_REQ:
					CompetitionEventHandle.Handle_SEASON_CHANNEL_SCHEDULE_REQ(this, b);
					break;
				case Opcodes.eServer_NEW_SEASON_PASS_USER_INFO_REQ:
					CompetitionEventHandle.Handle_NewSeasonPassUserInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_COMPETITION_EVENT_POINT_GATHERING_TEAM_POINT_REQ:
					CompetitionEventHandle.Handle_TeamPointGathering(this, b);
					break;
				case Opcodes.eServer_POINT_GATHERING_MY_INFO_REQ:
					CompetitionEventHandle.Handle_PointGatheringMyInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_FISHING_MY_RANKING_REQ:
					FishingHandle.Handle_MyRanking(this, b);
					break;
				case Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_MYINFO_REQ:
					TowerEvent.Handle_GetUserJoinInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_ENTER_REQ:
					TowerEvent.Handle_EnterEvent(this, packetReader, b);
					break;
				case Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_GIVEUP_REQ:
					TowerEvent.Handle_GiveUP(this, packetReader, b);
					break;
				case Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_GetBoxList_REQ:
					TowerEvent.Handle_UserGetItemInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_DOLIMPAN_MY_INFO_REQ:
					DolimpanHandle.Handle_GetMyInfo(this, b);
					break;
				case Opcodes.eServer_DOLIMPAN_TURN_BOARD_REQ:
					DolimpanHandle.Handle_Play(this, packetReader, b);
					break;
				case Opcodes.eServer_DOLIMPAN_RECEIVE_REWARD_REQ:
					DolimpanHandle.Handle_Reward(this, packetReader, b);
					break;
				case Opcodes.eServer_DOLIMPAN_RESET_REQ:
					DolimpanHandle.Handle_Reset(this, packetReader, b);
					break;
				case Opcodes.eServer_DOLIMPAN_USE_CHARGE_COUPON_REQ:
					DolimpanHandle.Handle_Charge(this, packetReader, b);
					break;
				case Opcodes.eServer_GUILD_PLANT_OPERATION_REQ:
					switch ((GuildPlantProtocol)packetReader.ReadLEInt16())
					{
					case GuildPlantProtocol.GET_GUILD_MANAGE_TR_REQ:
						GuildPlantHandle.Handle_GetGuildManageTR(this, packetReader, b);
						break;
					case GuildPlantProtocol.INVEST_GUILD_MANAGE_TR_REQ:
						GuildPlantHandle.Handle_InvestGuildManageTR(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_STORAGE_EXTEND_REQ:
						GuildPlantHandle.Handle_GetStorageExtend(this, packetReader, b);
						break;
					case GuildPlantProtocol.REGISTER_ITEM_REQ:
						GuildPlantHandle.Handle_RegisterItem(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_MAKE_PROGRESS_ITEM_REQ:
						GuildPlantHandle.Handle_GetMakeProgressItem(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_MAKE_STAND_BY_ITEM_LIST_REQ:
						GuildPlantHandle.Handle_GetMakeStandByItemList(this, packetReader, b);
						break;
					case GuildPlantProtocol.CHANGE_MY_CONSTRIBUTION_POINT_ITEM_REQ:
						GuildPlantHandle.Handle_ChangeMyConstributionPointItem(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_INVESTOR_MANAGE_TR_LIST_REQ:
						GuildPlantHandle.Handle_GetInvestorManageTRList(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_EXPENSE_LIST_REQ:
						GuildPlantHandle.Handle_GetExpenseList(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_ITEM_CONTRIBUTION_RANK_LIST_REQ:
						GuildPlantHandle.Handle_GetItemContributionRankList(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_GIVE_POSSIBLE_USER_LIST_REQ:
						GuildPlantHandle.Handle_GetGivePossibleUserList(this, packetReader, b);
						break;
					case GuildPlantProtocol.GIVE_GIFT_REQ:
						GuildPlantHandle.Handle_GiveGift(this, packetReader, b);
						break;
					case GuildPlantProtocol.GET_PLANT_ITEM_LIST_REQ:
						GuildPlantHandle.Handle_GetPlantItemList(this, packetReader, b);
						break;
					case GuildPlantProtocol.BUY_ITEM_REQ:
						GuildPlantHandle.Handle_BuyItem(this, packetReader, b);
						break;
					}
					break;
				case Opcodes.eServer_ALCHEMIST_MIX_REQ:
					AlchemistHandle.Handle_AlchemistMix(this, packetReader, b);
					break;
				case Opcodes.eServer_ALCHEMIST_ENCHANT_GRADE_REQ:
					AlchemistHandle.Handle_AlchemistEnchantGrade(this, packetReader, b);
					break;
				case Opcodes.eServer_ALCHEMIST_DISJOINT_REQ:
					AlchemistHandle.Handle_AlchemistDisjoint(this, packetReader, b);
					break;
				case Opcodes.eServer_ALCHEMIST_HISTORY_REQ:
					AlchemistHandle.Handle_AlchemistHistory(this, packetReader, b);
					break;
				case Opcodes.eServer_QUICK_JOIN_FOR_MAP_CARD_REQ:
					AlchemistHandle.Handle_AlchemistQuickJoin(this, packetReader, b);
					break;
				case Opcodes.eServer_NewAttendanceGetUserInfo:
					AttendanceHandle.Handle_NewAttendanceGetUserInfo(this, b);
					break;
				case Opcodes.eServer_ATTENDANCE_INFO_REQ:
					AttendanceHandle.Handle_AttendanceInfo(this, packetReader, b);
					break;
				case Opcodes.eServer_ATTENDANCE_REWARD_REQ:
					AttendanceHandle.Handle_AttendanceReward(this, packetReader, b);
					break;
				case Opcodes.eServer_EAC_MESSAGE_REQ:
					EACServer.OnReceivedEACMessage(this, packetReader);
					break;
				default:
					if (PackedAckMap.TrySendEmptyAck(this, wire, packetReader, b))
					{
						break;
					}
					if (LobbyUnhandledHandle.TryHandle(this, num, packetReader, b))
					{
						break;
					}
					if (Conf.ProtocolDebug)
					{
						Log.Warning("Unhandled opcode {0} wire-dump bytes {1} user:{2}", num, packetReader.Buffer.Length, CurrentAccount.UserID);
						ProtocolDump.Unhandled(wire, num, packetReader.Buffer, IP);
					}
					if (!CurrentAccount.isLogin)
					{
						DDOS_Filter(IP, 2);
					}
					break;
				case Opcodes.eServer_LOG_OUT_REQ:
				case Opcodes.eServer_RX_1490:
					PackedLogout(b, "LOG_OUT_REQ");
					break;
				case Opcodes.eServer_RX_1912:
					if (CurrentAccount != null && CurrentAccount.PackedOptionDumpUtc != DateTime.MinValue
						&& (DateTime.UtcNow - CurrentAccount.PackedOptionDumpUtc).TotalSeconds <= 2.0)
					{
						PackedLogout(b, "UDP_STATISTICS");
					}
					break;
				case Opcodes.eServer_RX_364:
				case Opcodes.eServer_RX_1042:
				case Opcodes.eServer_RX_1077:
				case Opcodes.eServer_RX_1441:
				case Opcodes.eServer_FISHING_FARM_MASTER_REWARD_ACK:
				case Opcodes.eServer_STORAGE_SAVE_ITEM_ACK:
				case Opcodes.eServer_DISCONNECT_FROM_SERVER_ACK:
					break;
				
			}
			}
			catch (Exception ex)
			{
				Log.Warning("HandleReceived exception opcode={0}: {1}", num, ex);
				try
				{
					string propertyValue = Utility.ByteArrayToString(packetReader.Buffer.ToArray());
					Log.Error("HandleReceived Error:{0} {1} {2}", ex.ToString(), num, propertyValue);
				}
				catch (Exception logEx)
				{
					Log.Warning("HandleReceived error-log failed: {0}", logEx.Message);
				}
			}
		}

		private void DDOS_Filter(string ip, int type)
		{
			try
			{
				if (!Conf.BlockDDOS)
				{
					return;
				}
				lock (DDOS_Lock)
				{
					if (DDOS_IP.TryGetValue(ip, out var value))
					{
						if (value.UnknownOpcodeTime >= Conf.JudgeTime || (DateTime.Compare(DateTime.Now, value.FirstTimeConnect.AddSeconds(10.0)) <= 0 && value.ConnectTime >= Conf.MaxConnectTime))
						{
							IRule rule = FirewallManager.Instance.Rules.FirstOrDefault((IRule r) => r.Name == "DDOS Block");
							if (rule != null)
							{
								List<IAddress> list = rule.RemoteAddresses.ToList();
								list.Add(SingleIP.FromIPAddress(IPAddress.Parse(ip)));
								rule.RemoteAddresses = list.ToArray();
								Log.Warning("Detected ip:{0} try to DDOS server!", ip);
							}
						}
						switch (type)
						{
						case 1:
							DDOS_IP[ip].ConnectTime++;
							break;
						case 2:
							DDOS_IP[ip].UnknownOpcodeTime++;
							break;
						}
						if (DateTime.Compare(DateTime.Now, value.FirstTimeConnect.AddSeconds(10.0)) > 0)
						{
							DDOS_IP[ip].FirstTimeConnect = DateTime.Now;
							DDOS_IP[ip].UnknownOpcodeTime = ((type == 2) ? 1 : 0);
							DDOS_IP[ip].ConnectTime = ((type == 1) ? 1 : 0);
						}
					}
					else
					{
						DDOS_IP.TryAdd(ip, new ConnectionInfo
						{
							UnknownOpcodeTime = ((type == 2) ? 1 : 0),
							ConnectTime = ((type == 1) ? 1 : 0),
							FirstTimeConnect = DateTime.Now
						});
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on filtering DDOS: ip:{1}\r\n{0}", ex.ToString(), ip);
			}
		}
	}
}
