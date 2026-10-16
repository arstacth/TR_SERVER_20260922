using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using NetMsg.LBS;
using NetMsg.Room;
using RoomServer.Holders;
using RoomServer.Packet;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using Serilog;
using TRCommon.Protocol;

namespace RoomServer
{
	public class AgentServer : ReceiveActor
	{
		public static Dictionary<int, IActorRef> AgentServerList = new Dictionary<int, IActorRef>();

		private static int ID = 1;

		public static ConcurrentDictionary<int, Account> CurrentAccounts { get; } = new ConcurrentDictionary<int, Account>();


		public AgentServer()
		{
			Receive<AgentConnectRequest>(delegate
			{
				int num = ID++;
				Log.Information("{0} AgentServer Connected!", num);
				AgentServerList.Add(num, base.Sender);
				Form1.UpdateLableStatic(num);
				base.Sender.Tell(new AgentConnectResponse
				{
					AgentID = num,
					ConnectedRoomID = ServerStatus.MyRoomServerID
				}, base.Self);
			});
			Receive(delegate(RM_Packet req)
			{
				Handle_RoomPacket(req.Session, req.data);
			});
			Receive(delegate(ReloadSetting re)
			{
				ReloadHandle(re);
			});
			Receive(delegate(TowerEventInfo re)
			{
				TowerEventHolder.TowerEventStatus(re.Type);
			});
			Receive(delegate(byte[] packet)
			{
				byte[] array = new byte[packet.Length];
				Buffer.BlockCopy(packet, 0, array, 0, packet.Length);
				HandlePacket(base.Sender, array);
			});
		}

		private void ReloadHandle(ReloadSetting re)
		{
			switch (re.Code)
			{
			case 5:
				MapHolder.LoadMapInfo();
				MapHolder.LoadMapRoomKind();
				break;
			case 6:
				GameRewardHolder.LoadGameRewardInfo();
				break;
			case 8:
				ServerSettingHolder.LoadServerSettingInfo();
				break;
			case 7:
				break;
			}
		}

		private void HandlePacket(IActorRef Sender, byte[] data)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			short num = packetReader.ReadLEInt16();
			byte last = packetReader.Buffer.LastOrDefault();
			switch ((RMProtocol)num)
			{
			case RMProtocol.RM_CreateRoom_REQ:
				GameRoomHandle.Handle_CreateGameRoom(Sender, packetReader, last);
				break;
			case RMProtocol.RM_UserLeaveRoom_REQ:
				GameRoomHandle.Handle_LeaveRoom(packetReader, last);
				break;
			case RMProtocol.RM_UserEnterRoom_REQ:
				GameRoomHandle.Handle_EnterRoom(Sender, packetReader, last);
				break;
			case RMProtocol.RM_KickPlayer_REQ:
				GameRoomHandle.Handle_KickPlayer(packetReader, last);
				break;
			case RMProtocol.RM_UserGameEndInfo_REQ:
				GameRoomHandle.Handle_GameEndInfo(packetReader, last);
				break;
			case RMProtocol.RM_GetPlayerPosList_REQ:
				GameRoomHandle.Handle_PlayerList(packetReader, last);
				break;
			case RMProtocol.RM_EnterFarm_PlayerInfo_REQ:
				FarmHandle.Handle_EnterFarm(Sender, packetReader, last);
				break;
			case RMProtocol.RM_EnterPublicFarm_PlayerInfo_REQ:
				FarmHandle.Handle_CreatePublicFarm(Sender, packetReader, last);
				break;
			case RMProtocol.RM_ReloadFarmMapInfo_REQ:
				FarmHandle.Handle_ReloadFarmMapInfo(packetReader, last);
				break;
			case RMProtocol.RM_ClearUserFarmMapInfo_REQ:
				FarmHandle.Handle_ClearUserFarmMapInfo(packetReader, last);
				break;
			case RMProtocol.RM_ModifyFarmMapInfo_REQ:
				FarmHandle.Handle_ModifyFarmMapInfo(packetReader, last);
				break;
			case RMProtocol.RM_IncreaseAnimalSize_REQ:
				FarmHandle.Handle_IncreaseAnimalSize(packetReader, last);
				break;
			case RMProtocol.RM_RestoreAnimalDefaultSize_REQ:
				FarmHandle.Handle_RestoreAnimalDefaultSize(packetReader, last);
				break;
			case RMProtocol.RM_ChangeFarmSkybox_REQ:
				FarmHandle.Handle_ChangeFarmSkybox(packetReader, last);
				break;
			case RMProtocol.AG_TO_RM_TO_User:
				Handle_Agent_To_RoomUser(packetReader);
				break;
			case RMProtocol.AG_TO_RM_TO_User_OnlyMe:
				Handle_Agent_To_RoomUser_Me(packetReader);
				break;
			case RMProtocol.RM_ChangeFarmMapTypeBySlot_REQ:
				FarmHandle.Handle_ChangeFarmMapTypeBySlot(packetReader, last);
				break;
			case RMProtocol.RM_GameRoomUpdateGuild_REQ:
				GameRoomHandle.Handle_UpdateGuild(packetReader, last);
				break;
			case RMProtocol.RM_LookingForGuildMatch_REQ:
				GuildMatchHandle.Handle_LookingForGuildMatch(packetReader, last);
				break;
			case RMProtocol.RM_PickGuildForMatch_REQ:
				GuildMatchHandle.Handle_PickGuildForMatch(packetReader, last);
				break;
			case RMProtocol.RM_CancelLookingForGuildMatch_REQ:
				GuildMatchHandle.Handle_CancelLookingForGuildMatch(packetReader, last);
				break;
			case RMProtocol.RM_ChooseGuildForMatch_REQ:
				GuildMatchHandle.Handle_ChooseGuildForMatch(packetReader, last);
				break;
			case RMProtocol.RM_PassVertification_REQ:
				GameRoomHandle.Handle_PassVertification(packetReader, last);
				break;
			case RMProtocol.RM_UpdateFarmFishingReward_REQ:
				FishingHandle.Handle_UpdateFarmFishingReward(packetReader, last);
				break;
			}
			packetReader.Offset = 2;
			if (num == (short)eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ)
			{
				RoomAgentProtocol_WRAP_ROOM_REQ(packetReader, last);
			}
		}

		private void Handle_RoomPacket(int Session, byte[] data)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			short num = packetReader.ReadLEInt16();
			byte last = packetReader.Buffer.LastOrDefault();
			if (!CurrentAccounts.TryGetValue(Session, out var value))
			{
				return;
			}
			switch ((RoomOpcodes)num)
			{
				case RoomOpcodes.eRoom_CHANGE_SLOT_STATE_REQ:
				RoomServerHandle.Handle_SlotControl(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_ROOMINFO_REQ:
				RoomServerHandle.Handle_ChangeSetting(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_TITLE_REQ:
				RoomServerHandle.Handle_ModifyTitle(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_PASSWORD_REQ:
				RoomServerHandle.Handle_ModifyPassword(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_ITEMMODE_REQ:
				RoomServerHandle.Handle_ModifyItemMode(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_STEPPINGMODE_REQ:
				RoomServerHandle.Handle_ModifyStepping(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_RUMBLE_GARDEN_ACTION_REQ:
				RoomServerHandle.Handle_RumbleGardenAction(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_RUMBLE_GARDEN_GOALIN_REQ:
				RoomServerHandle.Handle_RumbleGardenGoalIn(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_READY_REQ:
				RoomServerHandle.Handle_Ready(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MAP_CHANGE_REQ:
				RoomServerHandle.Handle_ChangeMap(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_REQUEST_START_COUNTING_REQ:
				RoomServerHandle.Handle_StartGame(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_USER_STATE:
				RoomServerHandle.Handle_ChangeStatus(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_START_LOADING_REQ:
				RoomServerHandle.Handle_StartLoading(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ALL_LOADING_END_REQ:
				RoomServerHandle.Handle_EndLoading(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_START_GAME_REQ:
				RoomServerHandle.Handle_GameStart(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GOAL_IN_REQ:
				RoomServerHandle.Handle_GoalInData(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TIME_OUT_RACE_LENGTH_REQ:
				GameModeHandle.GameMode_TimeOver(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_CHATTING:
				RoomServerHandle.Handle_RoomChat(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_FORWARD_TO_ALL_ROOM_USER_REQ:
				RoomServerHandle.Handle_MapControl(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TIME_COUNT_START_REQ:
				GameModeHandle.GameMode_LapTimeCountdwon(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_MAP_EVENT_REQ:
				RoomServerHandle.Handle_TriggerMapEvent(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ONE_IN_FOUR_KEY_IN_DOOR_REQ:
				RoomServerHandle.Handle_StepOnButton(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MASTER_REGISTER_REWARD_REQ:
				RoomServerHandle.Handle_RegisterItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GAMEUSER_REBIRTH_REQ:
				GameModeHandle.GameMode_MiniGame_Respawn(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MULTIMINIGAME_UPDATE_USER_POINT_REQ:
				GameModeHandle.GameMode_MiniGame_GetPoint(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GAMEROUND_INFO_SET:
				GameModeHandle.GameMode_MiniGame_RoundTime(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GAME_OVER_REQ:
				GameModeHandle.GameMode_GameOver(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_STEPPED_GOAL_BOARD_REQ:
				GameModeHandle.GameMode_FootStep_GoalIn(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ROOM_EVENT:
				GameModeHandle.GameMode_Amsan_LapTime(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_STEP_FOOT_BOARD_REQ:
				GameModeHandle.GameMode_Amsan_StepButton(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_PASS_FOOT_BOARD_AREA_REQ:
				GameModeHandle.GameMode_Amsan_StepButton_Push(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_END_GAME_BONUS_REQ:
				GameModeHandle.GameMode_Amsan_FinalButton(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_PASS_SURVIVAL_ARITHMETIC_CHECK_POINT_REQ:
				GameModeHandle.GameMode_Amsan_LapTimeControl(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_SURVIVAL_RANDOM_GAMEOVER_CHECK_POINT_REQ:
				GameModeHandle.GameMode_RandomGameOver(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_SURVIVAL_RANDOM_GAMEOVER_RACE_LENGTH_REQ:
				GameModeHandle.GameMode_RandomGameOver_Die(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_REMOVE_GAMEROOM_ITEM_REQ:
				RoomServerHandle.Handle_GiveUpItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_EAT_ITEM_REQ:
				RoomServerHandle.Handle_DrawItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_USE_GAMEROOM_ITEM_REQ:
				RoomServerHandle.Handle_UseItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MAKE_ITEM_CHECK_DUPLICATION_REQ:
				RoomServerHandle.Handle_RegItem2(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MAKE_ITEM_REQ:
				RoomServerHandle.Handle_RegItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_SELECT_TEAM_REQ:
				RoomServerHandle.Handle_ChangeTeam(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_SELECT_RELAY_TEAM_POSITION_REQ:
				RoomServerHandle.Handle_ChangeRelayTeam(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_SELECT_RELAY_TEAM_RANDOM_POSITION_REQ:
				RoomServerHandle.Handle_RandomChooseRelayTeam(value, last);
				break;
				case RoomOpcodes.eRoom_CHANGE_SLOT_STATE_RELAY_TEAM_REQ:
				RoomServerHandle.Handle_ChangeSlotStateRelay(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_RELAY_PREPARE_NEXT_RUNNER_REQ:
				RoomServerHandle.Handle_WaitPassBaton(value, last);
				break;
				case RoomOpcodes.eRoom_ENTER_BATON_TOUCH_AREA_REQ:
				RoomServerHandle.Handle_WaitPassBaton2(value, last);
				break;
				case RoomOpcodes.eRoom_LEAVE_BATON_TOUCH_AREA_REQ:
				RoomServerHandle.Handle_StartPassBaton(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_BATON_INPUT_REQ:
				RoomServerHandle.Handle_PassBaton(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MAKE_AND_EAT_ITEM_REQ:
				GameModeHandle.GameMode_CatchFish(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_RUN_QUIZMODE_REQUEST_QUIZ_LIST_REQ:
				GameModeHandle.RunQuizMode_RequestQuizList(value, last);
				break;
				case RoomOpcodes.eRoom_RABBIT_TURTLE_GET_ITEM:
				GameModeHandle.GameMode_TurtleEatItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_RABBIT_TURTLE_TAG_REQ:
				GameModeHandle.GameMode_ReqChangeTeamLeader(value, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_OBJECT_EVENT_REQ:
				GameModeHandle.CorunMode_TriggerObjectEvent(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_CHECK_IN_OBJECT_EVENT_REQ:
				GameModeHandle.CorunMode_TriggerCheckInObjectEvent(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_SET_CLEAR_LIMIT_TIME_REQ:
				GameModeHandle.CorunMode_SetClearLimitTime(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_ENTER_TIME_SECTION_REQ:
				GameModeHandle.CorunMode_EnterTimeSection(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_CLEAR_TIME_SECTION_REQ:
				GameModeHandle.CorunMode_ClearTimeSection(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_SET_BOSS_ENERGY_REQ:
				GameModeHandle.CorunMode_SetBossEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_DECREASE_BOSS_ENERGY_REQ:
				GameModeHandle.CorunMode_DecreaseBossEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_SET_OBJECT_BOSS_ENERGY_REQ:
				GameModeHandle.CorunMode_SetObjectBossEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_DECREASE_OBJECT_BOSS_ENERGY_REQ:
				GameModeHandle.CorunMode_DecreaseObjectBossEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_CORUN_MODE_INCREASE_OBJECT_BOSS_ENERGY_REQ:
				GameModeHandle.CorunMode_IncreaseObjectBossEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_SET_OBJECT_INFO_REQ:
				AssaultModeHandle.AssaultMode_SetObjectInfo(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_SET_CHARACTER_ENERGY_INFO_REQ:
				AssaultModeHandle.AssaultMode_SetCharacterEnergy(value, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_DECREASE_CHARACTER_ENERGY_REQ:
				AssaultModeHandle.AssaultMode_DecreaseCharacterEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_CHARGE_CHARACTER_ENERGY_REQ:
				AssaultModeHandle.AssaultMode_ChargeCharacterEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_DECREASE_OBJECT_ENERGY_REQ:
				AssaultModeHandle.AssaultMode_DecreaseObjectEnergy(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_GET_OBJECT_REWARD_REQ:
				AssaultModeHandle.AssaultMode_BounsItemMake(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_EAT_ITEM_REQ:
				AssaultModeHandle.AssaultMode_BounsItemEat(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_REBIRTH_REQ:
				AssaultModeHandle.AssaultMode_Rebirth(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_BONUS_ITEM_MAKE_REQ:
				AssaultModeHandle.Handle_InitMapBonusItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ASSAULT_MODE_BONUS_ITEM_EAT_REQ:
				AssaultModeHandle.Handle_MapBonusItemEat(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_FORWARD_TO_ROOM_USER_REQ:
				RoomServerHandle.Handle_setUserState(value, packetReader, last);
				break;
				case RoomOpcodes.eServer_FARM_CRAFT_PROTOCOL:
			{
				int num2 = packetReader.ReadLEInt32();
				switch ((eFarmCraftProtocol)num2)
				{
				case eFarmCraftProtocol.ModifyFarmMapInfo_REQ:
					FarmRoomHandle.Handle_FarmCraft_ModifyFarmMapInfo(value, packetReader, last);
					break;
				case eFarmCraftProtocol.ReloadMapInfo_REQ:
					FarmRoomHandle.Handle_ReloadMapInfo(value, packetReader, last);
					break;
				case eFarmCraftProtocol.SaveUserFarmSlotInfo_REQ:
					FarmRoomHandle.Handle_SaveUserFarmSlotInfo(value, packetReader, last);
					break;
				case eFarmCraftProtocol.ChangeFarmMapTypeBySlot_REQ:
					FarmRoomHandle.Handle_ChangeFarmMapTypeBySlot_New(value, packetReader, last);
					break;
				case eFarmCraftProtocol.ChangeFarmTypeByItem_REQ:
					FarmRoomHandle.Handle_ChangeFarmTypeByItem_New(value, packetReader, last);
					break;
				default:
					Log.Information("farm room opcode: {0}, {1}", num2, Utility.ByteArrayToString(packetReader.Buffer));
					break;
				}
				break;
			}
				case RoomOpcodes.eRoom_FARM_MODIFY_OBJECT_LOCK_INFO_REQ:
				FarmRoomHandle.Handle_FarmAction(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_FARM_CHANGE_FARM_NAME_REQ:
				FarmRoomHandle.Handle_ChangeFarmRoomName(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_FARM_MODIFY_OPTION_PUBLIC_REQ:
				FarmRoomHandle.Handle_ChangeFarmRoomPassword(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_FARM_MODIFY_OPTION_TALKING_REQ:
				FarmRoomHandle.Handle_PublicFarmRoom(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_WEDDING_CHANGE_AGREE_STATE_REQ:
				CoupleHandle.Handle_WeddingSetItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_WEDDING_AGREE_REQ:
				CoupleHandle.Handle_WeddingReady(value, last);
				break;
				case RoomOpcodes.eRoom_RX_1824:
				GameModeHandle.SubjectKing_GetQuestion(value, last);
				break;
				case RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_ACQUIRE_REQ:
				GameModeHandle.ItemRacing_GetAbility(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_FIRE_REQ:
				GameModeHandle.ItemRacing_UseAbility(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_REMOVE_REQ:
				GameModeHandle.ItemRacing_RemoveAbility(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GUILDMATCH_OTHER_PARTYINFO_REQ:
				RoomServerHandle.Handle_ChangeGuildMatchRoomName(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GUILDMATCH_REGIST_MATCH_REQ:
				RoomServerHandle.Handle_StartGuildMatching(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GUILDMATCH_UNREGIST_MATCH_REQ:
				RoomServerHandle.Handle_CancelGuildMatching(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GUILDMATCH_MATCH_ACTION_REQ:
				RoomServerHandle.Handle_ProcessInviteForGuildMatch(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MAKE_ITEM_FOR_MAP_GENERATE_REQ:
				RoomServerHandle.Handle_MapGenerateItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_EAT_ITEM_FOR_MAP_GENERATE_REQ:
				RoomServerHandle.Handle_PickMapItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_DROP_ITEM_FOR_MAP_GENERATE_REQ:
				RoomServerHandle.Handle_GiveUpMapItem(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TYPEINGRUN_QUESTION_REQ:
				GameModeHandle.TypingRun_ReqText(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TYPEINGRUN_TYPING_DONE_REQ:
				GameModeHandle.TypingRun_EnterText(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_BONUS_STAGE_REWARD_INFO_LIST_REQ:
				GameModeHandle.GetBonusStage_RewardList(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_BONUS_STAGE_LAST_CHANCE_POINT_REQ:
				GameModeHandle.GetBonusStage_ExtraPoint(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_ICEFLOWER_QUESTION_REQ:
				GameModeHandle.IceFlower_GetQuestion(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TYPEINGRUN_GOBLINRACING_QUESTION_REQ:
				GameModeHandle.TypingRun_GoblinRacingQuestion(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_TYPEINGRUN_GOBLINRACING_TYPING_DONE_REQ:
				GameModeHandle.TypingRun_GoblinRacingTypingDone(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_BOMB_COUTING_START_REQ:
				GameModeHandle.Bomb_CountingStart(value, last);
				break;
				case RoomOpcodes.eRoom_BOMB_RACE_TRANSFER_BOMB_REQ:
				GameModeHandle.Bomb_RaceTransferBomb(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GetBox:
				TowerEvent.Handle_GetBox(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GetBox2:
				TowerEvent.Handle_GetBox2(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_EnterEvent:
				TowerEvent.Handle_EnterEvent(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_GiveUP:
				TowerEvent.Handle_GiveUP(value, last);
				break;
				case RoomOpcodes.eRoom_UserGetItemInfo:
				TowerEvent.Handle_UserGetItemInfo(value, packetReader, last);
				break;
				case RoomOpcodes.eRoom_MASTER_ACTION_REQ:
				case RoomOpcodes.eRoom_GUILDMATCH_LEAVE_MATCH_ROOM_BRING_MYPARTY_REQ:
				case RoomOpcodes.eRoom_ABUSING_USER_LIST_REQ:
				case RoomOpcodes.eRoom_CURRENT_RACE_LENGTH_REQ:
				case RoomOpcodes.eRoom_HACKING_TOOL_CHECK_POINT_REQ:
				break;
			}
		}

		private void Handle_Agent_To_RoomUser(PacketReader reader)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				if (room != null)
				{
					ushort length = reader.ReadLEUInt16();
					byte[] np = reader.ReadByteArray(length);
					room.BroadcastToAll(np);
				}
			}
		}

		private void Handle_Agent_To_RoomUser_Me(PacketReader reader)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (CurrentAccounts.TryGetValue(key, out var value))
			{
				Rooms.GetRoom(value.CurrentRoomId);
				ushort length = reader.ReadLEUInt16();
				byte[] msg = reader.ReadByteArray(length);
				value.SendAsync(msg);
			}
		}

		private void RoomAgentProtocol_WRAP_ROOM_REQ(PacketReader reader, byte last)
		{
			reader.ReadLEInt16();
			switch ((RoomOpcodes)reader.ReadLEInt16())
			{
			case RoomOpcodes.eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT:
				ItemHandle.ActiveFuncItem_Timeout(reader, last);
				break;
			case RoomOpcodes.eRoom_CHANGE_USER_AVATAR_LOCK:
				AvatarLockHandle.Handle_CHANGE_USER_AVATAR_LOCK(reader, last);
				break;
			case RoomOpcodes.eRoom_CHANGE_USER_ITEM_ATTR:
				ItemHandle.Change_UserItemAttr(reader, last);
				break;
			case RoomOpcodes.eRoom_UPDATE_ITEM_ONOFF_INFO:
				MyRoomHandle.Handle_ItemOnOff(reader, last);
				break;
			case RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEMS:
				MyRoomHandle.Handle_ChangeUserActiveItems(reader, last);
				break;
			case RoomOpcodes.eRoom_CHANGE_USER_ACTIVE_ITEM_ONE:
				ItemHandle.Change_UserActiveItemOne(reader, last);
				break;
			case RoomOpcodes.eRoom_UPDATE_AVATAR_INFO:
				ItemHandle.UpdateAvatarInfo(reader, last);
				break;
			}
		}
	}
}
