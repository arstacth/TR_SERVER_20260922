using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Akka.Actor;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using NetMsg.Room;
using RoomServer.Holders;
using RoomServer.Packet.RoomServer;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.Map;
using RoomServer.Structuring.TypingRun;
using Serilog;

namespace RoomServer.Packet
{
	public class RoomServerHandle
	{
		public static void Handle_SlotControl(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null && User.RoomPos == room.RoomMasterIndex && room.is8Player && !room.isPlaying)
			{
				byte roompos = reader.ReadByte();
				bool isOff = reader.ReadBoolean();
				if (!room.PlayerList().Exists((Account p) => p.RoomPos == roompos) && room.SlotControl(roompos, isOff))
				{
					room.BroadcastToAll(new GameRoom_ControlRoomPos(roompos, isOff, last));
					room.BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(room));
				}
			}
		}

		private static bool CanModifyRoomOptions(Account User, NormalRoom room)
		{
			if (room == null || User == null || room.isPlaying || room.RoomKindID == 74 || room.PlayMode == 2)
			{
				return false;
			}
			return User.RoomPos == room.RoomMasterIndex;
		}

		private static string ReadOptionString(PacketReader reader)
		{
			if (reader == null || reader.Remaining < 2)
			{
				return string.Empty;
			}
			int len = reader.ReadLEInt16();
			if (len < 0)
			{
				len = 0;
			}
			if (len > reader.Remaining)
			{
				len = reader.Remaining;
			}
			if (len <= 0)
			{
				return string.Empty;
			}
			return reader.ReadBig5StringSafe(len);
		}

		private static void NotifyRoomSetting(NormalRoom room, byte last)
		{
			room.BroadcastToAll(new GameRoom_ChangeSetting(room, last));
			room.BroadcastToAgent(new RM_To_AG_ChangeSetting(room));
		}

		public static void Handle_ChangeSetting(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!CanModifyRoomOptions(User, room))
			{
				return;
			}
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			string password = string.Empty;
			if (num > 0)
			{
				password = reader.ReadBig5StringSafe(num);
			}
			bool isStepOn = reader.ReadBoolean();
			int itemType = reader.ReadLEInt32();
			room.setName(name);
			room.setPassword(password);
			room.setItemType(itemType);
			room.setIsStepOn(isStepOn);
			NotifyRoomSetting(room, last);
		}

		public static void Handle_ModifyTitle(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!CanModifyRoomOptions(User, room))
			{
				return;
			}
			string name = ReadOptionString(reader);
			room.setName(name);
			room.BroadcastToAll(new GameRoom_RoomOptionAck((ushort)RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_TITLE_ACK, name, last));
			room.BroadcastToAgent(new RM_To_AG_ChangeSetting(room));
		}

		public static void Handle_ModifyPassword(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!CanModifyRoomOptions(User, room))
			{
				return;
			}
			string password = ReadOptionString(reader);
			room.setPassword(password);
			room.BroadcastToAll(new GameRoom_RoomOptionAck((ushort)RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_PASSWORD_ACK, password, last));
			room.BroadcastToAgent(new RM_To_AG_ChangeSetting(room));
		}

		public static void Handle_ModifyItemMode(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!CanModifyRoomOptions(User, room))
			{
				return;
			}
			int itemType = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			room.setItemType(itemType);
			room.BroadcastToAll(new GameRoom_RoomOptionAck((ushort)RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_ITEMMODE_ACK, itemType, last));
			room.BroadcastToAgent(new RM_To_AG_ChangeSetting(room));
		}

		public static void Handle_ModifyStepping(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!CanModifyRoomOptions(User, room))
			{
				return;
			}
			bool stepOn = false;
			if (reader.Remaining >= 4)
			{
				stepOn = reader.ReadLEInt32() != 0;
			}
			else if (reader.Remaining >= 1)
			{
				stepOn = reader.ReadBoolean();
			}
			room.setIsStepOn(stepOn);
			room.BroadcastToAll(new GameRoom_RoomOptionAck((ushort)RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_STEPPINGMODE_ACK, stepOn, last));
			room.BroadcastToAgent(new RM_To_AG_ChangeSetting(room));
		}

		public static void Handle_RumbleGardenAction(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room == null || !User.InGame)
			{
				return;
			}
			int n = reader.Remaining;
			if (n > 0)
			{
				n--;
			}
			byte[] payload = n > 0 ? reader.ReadByteArray(n) : new byte[0];
			room.BroadcastToAll(new GameRoom_RumbleGardenActionAck(payload, last));
		}

		public static void Handle_RumbleGardenGoalIn(Account User, PacketReader reader, byte last)
		{
			Handle_GoalInData(User, reader, last);
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null)
			{
				room.BroadcastToAll(new GameRoom_RumbleGardenGoalInAck(User.RoomPos, last));
			}
		}

		public static void Handle_Ready(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			bool flag = reader.ReadBoolean();
			if (room != null && User.InGame && (!room.isPlaying || flag))
			{
				User.IsReady = flag;
				byte roomPos = User.RoomPos;
				room.BroadcastToAll(new GameRoom_RoomPosReady(roomPos, flag, last));
			}
		}

		public static void Handle_ChangeMap(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room == null)
			{
				return;
			}
			if (room.isPlaying || User.RoomPos != room.RoomMasterIndex)
			{
				return;
			}
			int num = reader.ReadLEInt32();
			num = MapHolder.ResolveChangeMap(room.RoomKindID, num, room.MapNum);
			if (num <= 0)
			{
				return;
			}
			if (!MapHolder.MapInfos.TryGetValue(num, out var _))
			{
				Log.Warning("Unknown MapNum: {0}, userNum: {1} (applying packed map change)", num, User.UserNum);
			}
			if ((!MapHolder.MapRoomKinds.TryGetValue(room.RoomKindID, out var value2) || !value2.Contains(num)) && ServerSettingHolder.ServerSettings.useMapNumByRoomKindIDCheck)
			{
				Log.Warning("invalid MapNum By RoomKind ID - userNum : {0}, roomKind : {1}, mapNum : {2} (applying packed map change)", User.UserNum, room.RoomKindID, num);
			}
			RoomHolder.RoomKindInfos.TryGetValue(room.RoomKindID, out var value3);
			if (value3.GameMode == 14)
			{
				num = room.MapNum;
			}
			room.setMapNum(num);
			room.BroadcastToAgent(new RM_To_AG_ChangeMap(room.ID, num));
			foreach (Account item in room.PlayerList())
			{
				item.SendAsync(new GameRoom_UnknownResponse(last));
				item.SendAsync(new GameRoom_ChangeMap_FF0906(num, last));
				item.SendAsync(new GameRoom_ChangeMap_FF4E03(item, num, last));
			}
		}

		public static void Handle_RoomChat(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.Offset += 2;
			short length = reader.ReadLEInt16();
			byte[] array = reader.ReadByteArray(length);
			if (User.InGame && array.Length != 0 && array.Length <= 120 && room != null)
			{
				room.BroadcastToAll(new GameRoom_RoomChat(User.RoomPos, array, last));
				if (room.RoomMasterIndex == User.RoomPos && !room.isPlaying && room.PlayMode != 2)
				{
					room.ResetAutoChangeRoomMaster();
				}
			}
		}

		public static void Handle_ChangeStatus(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int code = reader.ReadLEInt32();
			if (User.InGame)
			{
				room?.BroadcastToAll(new GameRoom_ChangeStatus(User.RoomPos, code, last));
			}
		}

		public static void Handle_StartGame(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null)
			{
				int mapNum = reader.ReadLEInt32();
				room.PressStartGame(User, mapNum, last);
			}
		}

		public static void Handle_StartLoading(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			int randseed = reader.ReadLEInt32();
			reader.ReadByte();
			if (room == null)
			{
				return;
			}
			if (!MapHolder.MapInfos.TryGetValue(num, out var value))
			{
				Log.Warning("StartLoading unknown MapNum {0} userNum {1} (packed UMT, continuing)", num, User.UserNum);
				value = new MapInfo
				{
					RuleType = 0,
					RuleType_New = new string[0]
				};
			}
			room.RuleType = value.RuleType;
			room.RuleType_New = value.RuleType_New;
			if (!room.is8Player && !room.CheckReadyPlayerNum(out var ErrorCode))
			{
				if (ErrorCode > 0)
				{
					User.SendAsync(new GameRoom_StartError(ErrorCode, last));
				}
				room.isPlaying = false;
				return;
			}
			if ((!MapHolder.MapRoomKinds.TryGetValue(room.RoomKindID, out var value2) || !value2.Contains(num)) && ServerSettingHolder.ServerSettings.useMapNumByRoomKindIDCheck)
			{
				Log.Warning("StartLoading invalid MapNum By RoomKind ID - userNum : {0}, roomKind : {1}, mapNum : {2} (continuing)", User.UserNum, room.RoomKindID, num);
			}
			if (!room.is8Player)
			{
				foreach (Account item in room.Players.Values.Where((Account p) => !p.IsReady && p.RoomPos != room.RoomMasterIndex).ToList())
				{
					room.KickPlayer(item, last);
				}
			}
			room.isPlaying = true;
			room.PlayingMapNum = num;
			room.Survival = (byte)room.PlayerCount();
			room.BonusStageSelect(randseed);
			room.BroadcastToAll(new GameRoom_StartLoading(room.PlayingMapNum, randseed, last));
			if (room.BuffType == 1)
			{
				room.BroadcastToAll(new GameRoom_PlayerBuff(room, last));
			}
			room.StartWaitAllSyncThread();
			room.SetRewardGroupList();
			if (room.GameMode == 14 && room.HasPassword && AssaultModeHandle.AssaultModeUseItem(User.UserNum, 800, out var itemnum, out var itemcount))
			{
				User.SendAsync(new AssaultModeUseItem_Ack(itemnum, itemcount, last));
				User.AgentConnect.Tell(new UpdateItemInfo
				{
					Session = User.Session,
					ItemNum = itemnum,
					Count = itemcount
				});
			}
		}

		public static void Handle_EndLoading(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			User.EndLoading = true;
			room.BroadcastToAll(new GameRoom_EndLoading(User.RoomPos, last));
		}

		public static void Handle_GameStart(Account User, PacketReader reader, byte last)
		{
			int iGameStartTick = reader.ReadLEInt32();
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || room == null || !room.isPlaying)
			{
				return;
			}
			room.CapsuleNum = num;
			room.StartGame();
			foreach (Account item in room.PlayerList())
			{
				item.SendAsync(new GameRoom_eRoom_START_GAME_ACK(item, iGameStartTick, num, last));
				item.SendAsync(new GameRoom_UnknownResponse2(last));
				item.GameEndType = 0;
				item.GameOver = false;
			}
			if (room.PlayingMapNum == 1606)
			{
				for (byte i = 0; i <= 7; i++)
				{
					Account Hare = room.PlayerList().FirstOrDefault((Account p) => p.RoomPos == i);
					if (Hare.Partner > 7)
					{
						Account account = (from p in room.PlayerList()
							where p.Team == Hare.Team && p.Partner > 7 && p.RoomPos != i
							select p into _
							orderby Guid.NewGuid()
							select _).FirstOrDefault();
						Hare.Animal = 0;
						Hare.Partner = account.RoomPos;
						Hare.TeamLeader = true;
						account.Animal = 1;
						account.Partner = Hare.RoomPos;
						account.TeamLeader = false;
					}
				}
				room.BroadcastToAll(new AllocatePartner(room, last));
				room.StartHareAndTortoiseThread();
			}
			if (room.RuleType == 8)
			{
				int num2 = 100000;
				foreach (Account item2 in room.PlayerList())
				{
					item2.CurrentLapTime = num2;
					item2.LastLapTime = 0;
					item2.SendAsync(new Amsan_LapTimeControl(0, num2, num2, isCorrect: false, last));
				}
			}
			if (TypingRunHolder.TypingRunMaps.Any((KeyValuePair<int, int> a) => a.Key == room.PlayingMapNum))
			{
				room.IsTypingRunMode = true;
				int value = TypingRunHolder.TypingRunMaps.FirstOrDefault((KeyValuePair<int, int> w) => w.Key == room.PlayingMapNum).Value;
				room.TypingRunText = (from w in TypingRunHolder.TypingRunTexts
					where w.MapNum == room.PlayingMapNum
					select w into _
					orderby Guid.NewGuid()
					select _).Take(value).ToList();
			}
			if (room.BonusStageLevel > 0)
			{
				room.StartBonusStageThread();
			}
		}

		public static void Handle_GoalInData(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null && User.InGame && room.GameMode != 38 && room.isPlaying)
			{
				int laptime = reader.ReadLEInt32();
				int currentTime = room.GetCurrentTime();
				room.GoalIn(User, laptime, currentTime, last);
			}
		}

		public static void Handle_MapControl(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && room != null)
			{
				short num = reader.ReadLEInt16();
				byte[] unk = reader.ReadByteArray(num);
				room.BroadcastToAll(new GameRoom_MapControl(num, unk, last));
			}
		}

		public static void Handle_TriggerMapEvent(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.Offset++;
			byte eventnum = reader.ReadByte();
			int eventlaptime = reader.ReadLEInt32();
			room.BroadcastToAll(new GameRoom_TriggerMapEvent(eventnum, eventlaptime, last));
		}

		public static void Handle_GiveUpItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null)
			{
				bool realitem = User.RealItem;
				if (room.RuleType == 45568 || room.RuleType == 111104)
				{
					realitem = true;
				}
				room.BroadcastToAll(new GameRoom_GiveUpItem(User, realitem, last));
			}
		}

		public static void Handle_DrawItem(Account User, PacketReader reader, byte last)
		{
			int time = reader.ReadLEInt32();
			int capsuleID = reader.ReadLEInt32();
			byte rank = reader.ReadByte();
			bool bMakeAndEat = reader.ReadByte() == 1;
			int fixitem = reader.ReadLEInt32();
			reader.ReadLEInt32();
			Rooms.GetRoom(User.CurrentRoomId)?.DrawItem(User, time, capsuleID, rank, bMakeAndEat, fixitem, last);
		}

		public static void Handle_UseItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.ReadLEInt32();
			int time = reader.ReadLEInt32();
			int itemid = reader.ReadLEInt32();
			short length = reader.ReadLEInt16();
			room.BroadcastToAll(new GameRoom_UseItem(bytes: reader.ReadByteArray(length), pos: User.RoomPos, time: time, itemid: itemid, last: last));
		}

		public static void Handle_RegItem(Account User, PacketReader reader, byte last)
		{
			int time = reader.ReadLEInt32();
			int itemid = reader.ReadLEInt32();
			short length = reader.ReadLEInt16();
			byte[] bytes = reader.ReadByteArray(length);
			Rooms.GetRoom(User.CurrentRoomId)?.RegItem(User, time, itemid, bytes, last);
		}

		public static void Handle_RegItem2(Account User, PacketReader reader, byte last)
		{
			int time = reader.ReadLEInt32();
			int itemid = reader.ReadLEInt32();
			reader.ReadLEInt32();
			short length = reader.ReadLEInt16();
			byte[] bytes = reader.ReadByteArray(length);
			Rooms.GetRoom(User.CurrentRoomId)?.RegItem2(User, time, itemid, bytes, last);
		}

		public static void Handle_ChangeTeam(Account User, PacketReader reader, byte last)
		{
			byte b = reader.ReadByte();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if ((User.RoomPos == room.RoomMasterIndex || !User.IsReady) && User.Team != b)
			{
				User.Team = b;
				room.BroadcastToAll(new GameRoom_RoomPosTeam(User, last));
			}
		}

		public static void Handle_StepOnButton(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte[] unk = reader.ReadByteArray(12);
			room.BroadcastToAll(new GameRoom_StepOnButton(unk, last));
		}

		public static void Handle_RegisterItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room == null)
			{
				return;
			}
			int num = reader.ReadLEInt32();
			long num2 = reader.ReadLEInt64();
			reader.ReadLEInt32();
			int isorderby = reader.ReadLEInt32();
			int sendrank = reader.ReadLEInt32();
			bool ispublic = reader.ReadBoolean();
			if (room.ItemNum != -1)
			{
				User.SendAsync(new GameRoom_LockKeepItem(room, isCancel: true, last));
			}
			if (!CheckRegisterItem(User.UserNum, num2))
			{
				return;
			}
			room.RegisterItem(num, num2, isorderby, sendrank, ispublic);
			foreach (Account item in room.PlayerList())
			{
				item.SendAsync(new GameRoom_RegisterSuccess(num, num2, last));
				item.SendAsync(new GameRoom_GoodsInfo(room, last));
			}
			if (num != -1)
			{
				User.SendAsync(new GameRoom_LockKeepItem(room, isCancel: false, last));
			}
		}

		public static void Handle_ChangeRelayTeam(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && !room.isPlaying && room.GameMode == 3 && room != null)
			{
				byte relayteam = reader.ReadByte();
				if (!room.Players.Values.Any((Account e) => e.RelayTeamPos == relayteam && relayteam > 2))
				{
					User.SelectRelayTeam(relayteam);
					room.BroadcastToAll(new GameRoom_RoomPosRelayTeam(User, last));
				}
			}
		}

		public static void Handle_RandomChooseRelayTeam(Account User, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room.isPlaying)
			{
				return;
			}
			IOrderedEnumerable<byte> source = from w in room.RelayPosList
				where !room.Players.Values.Any((Account s) => s.RelayTeamPos == w)
				select w into _
				orderby Guid.NewGuid()
				select _;
			if (source.Count() != 0)
			{
				User.SelectRelayTeam(source.FirstOrDefault());
				room.BroadcastToAll(new GameRoom_RoomPosRelayTeam(User, last));
			}
		}

		public static void Handle_ChangeSlotStateRelay(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte b = reader.ReadByte();
			bool flag = reader.ReadBoolean();
			if (flag && room.RelayPosList.Count <= 6)
			{
				User.SendAsync(new GameRoom_RelaySlotCannotChange(last));
				return;
			}
			room.setRelayPosWeight(b, flag);
			room.BroadcastToAll(new GameRoom_RelayChangeSlotState(b, flag, last));
			room.BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(room));
		}

		public static void Handle_WaitPassBaton(Account User, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).BroadcastToAll(new GameRoom_WaitPassBaton(User, last));
		}

		public static void Handle_WaitPassBaton2(Account User, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId).BroadcastToAll(new GameRoom_WaitPassBaton2(User, last));
		}

		public static void Handle_PassBaton(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte teampos = reader.ReadByte();
			int unk = reader.ReadLEInt32();
			room.BroadcastToAll(new GameRoom_PassBaton(User, teampos, unk, last));
		}

		public static void Handle_StartPassBaton(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.ReadLEInt32();
			room.BroadcastToAll(new GameRoom_StartPassBaton(User, last));
		}

		public static void Handle_setUserState(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && room != null)
			{
				reader.Offset++;
				short length = reader.ReadLEInt16();
				byte[] array = reader.ReadByteArray(length);
				room.BroadcastToAll(new GameRoom_UserState(User.RoomPos, array, last));
			}
		}

		public static void Handle_ChangeGuildMatchRoomName(Account User, PacketReader reader, byte last)
		{
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room != null)
			{
				room.setName(name);
				room.BroadcastToAll(new GameRoom_ChangeGuildMatchRoomName(name, last));
				User.SendAsync(new GameRoom_UpdateGuildMatchRoomInfo(room, last));
			}
		}

		public static void Handle_StartGuildMatching(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			short num = reader.ReadLEInt16();
			reader.ReadLEInt32();
			bool flag = room.Players.Values.Count((Account p) => p.IsReady && p.RoomPos != room.RoomMasterIndex && p.Attribute != 3) + 1 == ServerSettingHolder.ServerSettings.GuildMatchPartyMemberCount;
			room.BroadcastToAll(new GameRoom_StartGuildMatching(num, flag, last));
			if (flag)
			{
				room.GuildMatchMode = num;
				room.IsSearchGuildMatch = true;
				room.RandomSearchTime = 0;
			}
		}

		public static void Handle_CancelGuildMatching(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.ReadLEInt16();
			reader.ReadLEInt32();
			room.IsSearchGuildMatch = false;
			room.BroadcastToAll(new GameRoom_CancelGuildMatching(last));
		}

		public static void Handle_ProcessInviteForGuildMatch(Account User, PacketReader reader, byte last)
		{
			Rooms.GetRoom(User.CurrentRoomId);
			short status = reader.ReadLEInt16();
			Rooms.GetRoomForGuild(reader.ReadLEInt32());
			User.SendAsync(new GameRoom_ProcessInviteForGuildMatch(status, last));
		}

		public static void Handle_BackPrepareGuildMatch(Account User, byte last)
		{
		}

		public static void Handle_MapGenerateItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int unk = reader.ReadLEInt32();
			short num = reader.ReadLEInt16();
			short length = reader.ReadLEInt16();
			byte[] buffer = reader.ReadByteArray(length);
			int num2 = room.MapItem.Count + 1;
			room.MapItem.TryAdd(num2, num);
			room.BroadcastToAll(new GameRoom_MapGenerateItem(unk, num, num2, buffer, last));
		}

		public static void Handle_PickMapItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			if (room.MapItem.TryGetValue(num, out var value))
			{
				room.MapItem.TryRemove(num, out var _);
				room.BroadcastToAll(new GameRoom_PickMapItem(User.RoomPos, num, value, last));
			}
		}

		public static void Handle_GiveUpMapItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int time = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			Dictionary<int, short> dictionary = new Dictionary<int, short>();
			for (int i = 1; i <= num; i++)
			{
				int num2 = reader.ReadLEInt32();
				int key = reader.ReadLEInt32();
				reader.ReadLEInt32();
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, (short)num2);
				}
				room.MapItem.TryAdd(key, (short)num2);
			}
			room.BroadcastToAll(new GameRoom_GiveUpMapItem(dictionary, time, last));
		}

		private static bool CheckRegisterItem(int usernum, long storageid)
		{
			if (storageid == -1)
			{
				return true;
			}
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_storage_checkroomreward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
				mySqlCommand.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = storageid;
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToByte(mySqlDataReader["retval"]) == 0)
				{
					return true;
				}
				mySqlCommand.Dispose();
				mySqlDataReader.Close();
				mySqlConnection.Close();
			}
			return false;
		}
	}
}
