using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentServer.Function;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Shu;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using LocalCommons.Utilities;
using NestedDictionaryLib;
using Serilog;
using TRCommon;
using Weighted_Randomizer;

namespace AgentServer.Structuring
{
	public class Account
	{
		public AdvancedAvatarInfo advancedAvatarInfo = new AdvancedAvatarInfo();

		public readonly object fishingLock = new object();

		public bool TowerEventJoined;

		public global::AgentServer.Structuring.Mission.DailyMissionInfo DailyMission = new global::AgentServer.Structuring.Mission.DailyMissionInfo();

		public UserMissionInfo Mission = new UserMissionInfo();

		public DateTime DailyMissionStartTime = DateTime.Now;

		public ConcurrentDictionary<int, QuestInfo> QuestInfo = new ConcurrentDictionary<int, QuestInfo>();

		public List<TRADE_RESULT> TRADE_RESULT;

		public eSTATE CubeState;

		public CUBE_INFO_PROCESS CubeProcess;

		private byte m_iBonusRate = 1;

		public readonly object missionLock = new object();

		public int UserNum { get; set; }

		public string UserID { get; set; } = string.Empty;


		public int RoomServerID { get; set; }

		public List<UserItemDyeing> AvatarItemDyeing { get; } = CreateAvatarDyeingSlots();

		private static List<UserItemDyeing> CreateAvatarDyeingSlots()
		{
			List<UserItemDyeing> list = new List<UserItemDyeing>(24);
			for (int i = 0; i < 24; i++)
			{
				list.Add(new UserItemDyeing());
			}
			return list;
		}

		public void EnsureAvatarDyeingSlots()
		{
			while (AvatarItemDyeing.Count < 24)
			{
				AvatarItemDyeing.Add(new UserItemDyeing());
			}
		}


		public string LoginKey { get; set; }

		public NestedDictionary<byte, int, UserItemDyeing> DyedItemList { get; set; } = new NestedDictionary<byte, int, UserItemDyeing>();


		public CActiveItems activeItem { get; set; } = new CActiveItems();


		public CUserItemAttrManager userItemAttr { get; set; } = new CUserItemAttrManager();


		public CUserItemAttrManager mixedItemAttr { get; set; } = new CUserItemAttrManager();


		public CharAbilityAttr charAbilityAttr { get; set; } = new CharAbilityAttr();


		public CAvatarLock avatarLock { get; set; } = new CAvatarLock();


		public ConcurrentDictionary<int, UserEnchantItem> enchantItemList { get; set; } = new ConcurrentDictionary<int, UserEnchantItem>();


		public bool isRecvAllofCharItem { get; set; }

		public ConcurrentDictionary<int, ItemStrengthen> UserItemStrengthen { get; } = new ConcurrentDictionary<int, ItemStrengthen>();


		public NestedDictionary<int, byte, byte> UserItemStrengthenSlotGroup { get; } = new NestedDictionary<int, byte, byte>();


		public ConcurrentDictionary<int, List<ItemAttr>> UserItemStrengthenSlotAttr { get; } = new ConcurrentDictionary<int, List<ItemAttr>>();


		public ConcurrentDictionary<int, List<ItemAttr>> UserItemStrengthenSlotAttrGroup { get; } = new ConcurrentDictionary<int, List<ItemAttr>>();


		public int Session { get; set; }

		public ClientConnection Connection { get; set; }

		public string NickName { get; set; }

		public long Exp { get; set; }

		public long TR { get; set; }

		public int Cash { get; set; }

		public int Level { get; set; }

		public decimal Luck { get; set; }

		public string LastIp { get; set; }

		public short Port { get; set; }

		public byte[] UDPInfo { get; set; }

		public bool isLogin
		{
			get
			{
				if (bLogin)
				{
					return !isWaitLogin;
				}
				return false;
			}
		}

		public bool bLogin { get; set; }

		/// <summary>True only after LOGIN_AUTH password check succeeds. Wrong password must not reach LOGIN_REQ.</summary>
		public bool LoginAuthPassed { get; set; }

		public bool isWaitLogin { get; set; }

		public bool noNickName { get; set; }

		public byte[] EncryptKey { get; set; }

		public byte[] XorKey { get; set; }

		public int Attribute { get; set; }

		public int TopRank { get; set; }

		public int GameOption { get; set; }

		public long CurrentShuID { get; set; }

		public UserShuInfo UserShuInfo { get; set; } = new UserShuInfo();


		public UserCoupleInfo CoupleInfo { get; set; } = new UserCoupleInfo();


		public DateTime LoginDateTime { get; set; }

		public long LastCheckTime { get; set; }

		public long LastPingTime { get; set; }

		public bool isBlocked { get; set; }

		public bool isDisconnected { get; set; }

		public bool ChangedTalesBook { get; set; }

		public bool TRNeedUpdateFromDB { get; set; }

		public bool EXPNeedUpdateFromDB { get; set; }

		public bool CashNeedUpdateFromDB { get; set; }

		public bool ItemNeedUpdateFromDB { get; set; }

		public bool CardUpdate { get; set; }

		public int FreePassType { get; set; }

		public long ChallengeStartTime { get; set; }

		public int ChallengeMapNum { get; set; }

		/// <summary>StatSystem unlocked page slots (buy increases). Default 1.</summary>
		public int StatSystemPageCount { get; set; } = 1;

		/// <summary>StatSystem page titles (0-based). Persist for MyInfo reload.</summary>
		public string[] StatSystemTitles { get; } = new string[10];

		public int SinglePlayMapNum { get; set; }

		public short PartyType { get; set; }

		public int SubPartyType { get; set; }

		/// <summary>Once per session: already-joined Animal Village sent PartyJoinOK to skip intro.</summary>
		public bool CompetitionEventJoinAckSent { get; set; }

		public bool IsReady { get; set; }

		public bool InGame { get; set; }

		public int CurrentRoomId { get; set; }

		public byte RoomPos { get; set; }

		public bool EndLoading { get; set; }

		public int LapTime { get; set; }

		public int ServerLapTime { get; set; }

		public short GameEndType { get; set; }

		public float RaceDistance { get; set; }

		public byte Rank { get; set; }

		public byte Team { get; set; }

		public bool GameOver { get; set; }

		public int CurrentLapTime { get; set; }

		public int LastLapTime { get; set; }

		public byte RelayTeamPos { get; set; }

		public byte RelayTeam { get; set; }

		public int Partner { get; set; } = 8;


		public int Animal { get; set; }

		public float Fatigue { get; set; }

		public bool TeamLeader { get; set; }

		public bool RequestChange { get; set; }

		public int HP { get; set; }

		public int MaxHP { get; set; }

		public int TotalDamage { get; set; }

		public int MaxDamage { get; set; }

		public int RebirthTime { get; set; }

		public int MyFarmUniqueNum { get; set; }

		public MyFarmInfo MyFarmInfo { get; set; }

		public bool isFishing { get; set; }

		public int FishingRodItemNum { get; set; }

		public int FishingBaitItemNum { get; set; }

		public byte FishAns1 { get; set; }

		public byte FishAns2 { get; set; }

		public CancellationTokenSource FishingCancelSource { get; set; }

		public CancellationToken FishingCancelToken { get; set; }

		public int FishedItemNum { get; set; }

		public int FishedSize { get; set; }

		public long FishedKeepNum { get; set; }

		public byte RandomFish { get; set; }

		public int MatchTime { get; set; }

		public byte VertificationCode { get; set; }

		public bool NeedVertificated { get; set; }

		public byte WrongTime { get; set; }

		public Dictionary<int, int> LapinTime { get; } = new Dictionary<int, int>();


		public int RunlympicPoint { get; set; }

		public int CurrentLap { get; set; } = 1;


		public long StartLapTime { get; set; }

		public long EndLapTime { get; set; }

		public int CurrentPartyID { get; set; }

		public int GuildNum { get; set; } = -1;


		public GuildInfo GuildInfo { get; set; }

		public GuildUserInfo GuildUserInfo { get; set; } = new GuildUserInfo();


		public int CurrentGuildMatchRoomId { get; set; }

		public bool RealItem { get; set; }

		public int EventPickBoardStatus { get; set; }

		public int CuerrentPickBoardNum { get; set; }

		public int CuerrentPickBoardRemain { get; set; } = 50;


		public bool CuerrentPickBoardIsReward1St { get; set; }

		public int MileagePoint { get; set; }

		public int VipLevel { get; set; } = 1;


		public long EAC_LastSendTime { get; set; } = Utility.CurrentTimeMilliseconds();


		public DateTime LastSelectMachineTime { get; set; } = DateTime.Now;

		public DateTime PackedOptionDumpUtc { get; set; } = DateTime.MinValue;


		public byte SequenceNum { get; set; }

		public bool GotClientKey { get; set; }

		public bool isInRoom(out NormalRoom room)
		{
			room = Rooms.GetRoom(CurrentRoomId);
			if (InGame)
			{
				return room != null;
			}
			return false;
		}

		public void resetDolimpanDoubleBonus()
		{
			m_iBonusRate = 1;
		}

		public void setDolimpanDoubleBonus()
		{
			m_iBonusRate = 2;
		}

		public byte getDolimpanBonusRate()
		{
			return m_iBonusRate;
		}

		public bool isDolimpanDoubleBonus()
		{
			return 2 == m_iBonusRate;
		}

		public void SelectRelayTeam(byte relayteampos)
		{
			IsReady = relayteampos > 2;
			RelayTeamPos = relayteampos;
			switch (RelayTeamPos)
			{
			case 1:
			case 2:
				RelayTeam = 0;
				break;
			case 3:
			case 4:
			case 5:
				RelayTeam = 1;
				break;
			case 6:
			case 7:
			case 8:
				RelayTeam = 2;
				break;
			case 9:
			case 10:
			case 11:
				RelayTeam = 3;
				break;
			case 12:
			case 13:
			case 14:
				RelayTeam = 4;
				break;
			case 15:
			case 16:
			case 17:
				RelayTeam = 5;
				break;
			case 18:
			case 19:
			case 20:
				RelayTeam = 6;
				break;
			default:
				RelayTeam = 0;
				break;
			}
		}

		public void LeaveRoomReset()
		{
			InGame = false;
			IsReady = false;
			RoomPos = 0;
			CurrentRoomId = 0;
			Team = 0;
			RelayTeam = 0;
			RelayTeamPos = 0;
			EndLoading = false;
			Partner = 8;
			Fatigue = 0f;
			TeamLeader = false;
			TotalDamage = 0;
			MaxDamage = 0;
			RebirthTime = 0;
			GameEndType = 0;
			RaceDistance = 0f;
			LapinTime.Clear();
			RunlympicPoint = 0;
			RealItem = false;
		}

		public void GetMyLevel()
		{
			Level = AccountHolder.LevelInfo.Count((long c) => c <= Exp) + 1;
		}

		public void resetCouple()
		{
			CoupleInfo.CoupleNum = -1;
			CoupleInfo.MateName = string.Empty;
			CoupleInfo.CreateTime = 0L;
			CoupleInfo.CoupleRingNum = -1;
			CoupleInfo.CondDays = -1;
			CoupleInfo.CoupleLevel = -1;
			CoupleInfo.MaxRingDays = -1;
			CoupleInfo.CoupleType = 0;
			CoupleInfo.MarriedTime = 0L;
			CoupleInfo.RingChangedTime = 0L;
			CoupleInfo.AccumulateExp = 0;
			CoupleInfo.CouplePoint = 0;
		}

		public void setCoupleType(int coupleType)
		{
			CoupleInfo.CoupleType = coupleType;
		}

		public void StartFishingThread()
		{
			Task.Run(delegate
			{
				FishingThread(FishingCancelToken);
			});
		}

		private async void FishingThread(CancellationToken token)
		{
			int fisheditemnum;
			while (isFishing)
			{
				try
				{
					int num = 0;
					List<int> onItemList = activeItem.getOnItemList(185);
					List<int> onItemList2 = activeItem.getOnItemList(186);
					int usingequip = onItemList.FirstOrDefault();
					int usingbait = onItemList2.FirstOrDefault();
					// Park cast sends rod/bait on start REQ; active-item slots 185/186 are often empty.
					if (usingequip <= 0 && FishingRodItemNum > 0)
					{
						usingequip = FishingRodItemNum;
					}
					if (usingbait <= 0 && FishingBaitItemNum > 0)
					{
						usingbait = FishingBaitItemNum;
					}
					if (usingequip <= 0 && FishingHolder.FishingEquips.Count > 0)
					{
						usingequip = FishingHolder.FishingEquips.Keys.First();
					}
					if (usingbait <= 0 && FishingHolder.FishingBaitsDrawItem.Count > 0)
					{
						usingbait = FishingHolder.FishingBaitsDrawItem.Keys.First();
					}
					if (usingequip <= 0 || usingbait <= 0)
					{
						num = 291;
					}
					FishingEquip value;
					bool num2 = FishingHolder.FishingEquips.TryGetValue(usingequip, out value);
					IWeightedRandomizer<Decoy> drawitem;
					bool flag = FishingHolder.FishingBaitsDrawItem.TryGetValue(usingbait, out drawitem);
					if (num == 0 && !num2)
					{
						num = 291;
					}
					else if (num == 0 && !flag)
					{
						num = 292;
					}
					if (num > 0)
					{
						Connection.SendAsync(new FishedItemFail(num, 1));
						isFishing = false;
						if (FishingCancelSource != null)
						{
							FishingCancelSource.Cancel();
						}
						break;
					}
					bool isMiniGameDecoy = FishingHolder.Decoy_Ex.ContainsKey(usingbait);
					Random rand = new Random(Guid.NewGuid().GetHashCode());
					int num3 = rand.Next(value.MinSec, value.MaxSec) * 1000;
					if (isMiniGameDecoy)
					{
						FishingHolder.Decoy_Ex.TryGetValue(usingbait, out var value2);
						num3 -= rand.Next(value2.CatchingMinTime, value2.CatchingMaxTime);
					}
					await Task.Delay(num3, token).ContinueWith(delegate
					{
					});
					if (token.IsCancellationRequested || !isFishing)
					{
						break;
					}
					NormalRoom room = Rooms.GetRoom(CurrentRoomId);
					if (room == null)
					{
						Connection.SendAsync(new FishedItemFail(295, 1));
						isFishing = false;
						if (FishingCancelSource != null)
						{
							FishingCancelSource.Cancel();
						}
						break;
					}
					int num4 = 0;
					fisheditemnum = 0;
					int num5 = 0;
					bool isFarmReward = false;
					if (room.RoomKindID == 75 && room.FarmIndex != MyFarmUniqueNum && rand.Next(0, 100) + 1 > 97)
					{
						FishingHandle.storage_GetFarmFishingReward(room.FarmIndex, out var farmRewardList);
						if (farmRewardList.Count > 0)
						{
							UserStorageItemInfo userStorageItemInfo = farmRewardList.OrderBy((UserStorageItemInfo _) => Guid.NewGuid()).FirstOrDefault();
							fisheditemnum = userStorageItemInfo.itemNum;
							if (!isMiniGameDecoy)
							{
								num4 = FishingHandle.FishingSuccess(UserNum, userStorageItemInfo.itemNum, 0, isFish: false, fisheditemnum, userStorageItemInfo.uniqueNum, usingequip);
								isFarmReward = true;
								if (num4 == 0)
								{
									ServerStatus.ToRoomServer(new RM_UpdateFarmFishingReward(this, farmRewardList.Count - 1 > 0, 1), room.RoomServerID);
								}
								goto IL_05db;
							}
							FishingHolder.Decoy_Ex.TryGetValue(usingbait, out var value3);
							FishedItemNum = fisheditemnum;
							FishedSize = 0;
							FishedKeepNum = userStorageItemInfo.uniqueNum;
							RandomFish = (byte)rand.Next(1, 4);
							Connection.SendAsync(new MiniGameFishing_Ready(fisheditemnum, 0, usingbait, RandomFish, value3.MiniGameType, 16));
							room.BroadcastToAll(Session, new MiniGameFishing_ReadyNotify(RoomPos, FishedItemNum, 0, 16).ToArray());
						}
					}
					Decoy decoy = drawitem.NextWithReplacement();
					fisheditemnum = decoy.FishNum;
					bool flag2 = FishingHolder.Fishes.ContainsKey(fisheditemnum);
					if (flag2)
					{
						FishingHolder.Fishes.TryGetValue(decoy.FishNum, out var value4);
						num5 = FishingHandle.FishSizeCalc(rand, value4.MinSize, value4.MaxSize);
						fisheditemnum = ItemHolder.ItemShopInfos.Where((KeyValuePair<int, ItemShopInfo> w) => w.Value.supplyItemDescNum == fisheditemnum).FirstOrDefault().Key;
					}
					if (!isMiniGameDecoy)
					{
						num4 = FishingHandle.FishingSuccess(UserNum, fisheditemnum, num5, flag2, decoy.FishNum, 0L, usingequip);
					}
					else
					{
						FishingHolder.Decoy_Ex.TryGetValue(usingbait, out var value5);
						FishedItemNum = decoy.FishNum;
						FishedSize = num5;
						FishedKeepNum = 0L;
						RandomFish = (byte)rand.Next(1, 4);
						Connection.SendAsync(new MiniGameFishing_Ready(FishedItemNum, FishedSize, usingbait, RandomFish, value5.MiniGameType, 16));
						room.BroadcastToAll(Session, new MiniGameFishing_ReadyNotify(RoomPos, FishedItemNum, FishedSize, 16).ToArray());
					}
					goto IL_05db;
					IL_05db:
					if (!isMiniGameDecoy)
					{
						if (num4 != 0)
						{
							int err = 287;
							switch (num4)
							{
							case 1:
								err = 296;
								break;
							case 2:
								err = 292;
								break;
							case 3:
								err = 289;
								break;
							case 4:
								err = 299;
								break;
							}
							Connection.SendAsync(new FishedItemFail(err, 1));
							isFishing = false;
							if (FishingCancelSource != null)
							{
								FishingCancelSource.Cancel();
							}
							break;
						}
						byte[] packet = new PlayerFishedItem(this, fisheditemnum, num5, isFarmReward, 1).ToArray();
						room.BroadcastToAll(Session, packet);
						Connection.SendAsync(new FishedItem(fisheditemnum, num5, usingbait, 1));
					}
					else
					{
						int elapsed_time = 0;
						while (FishedItemNum != 0 && elapsed_time < 60 && isFishing)
						{
							elapsed_time++;
							await Task.Delay(1000);
						}
						if (FishedItemNum != 0 && elapsed_time >= 60)
						{
							FishedItemNum = 0;
							FishedKeepNum = 0L;
							FishedSize = 0;
							int err2 = 288;
							if (FishingCancelSource != null)
							{
								FishingCancelSource.Cancel();
							}
							Connection.SendAsync(new FishedItemFail(err2, 1));
							Log.Warning("Player [{0}] mini game fishing timed-out!", NickName);
							break;
						}
					}
					drawitem = null;
				}
				catch (Exception ex)
				{
					Log.Error("Error on player fishing: {0}", ex.ToString());
					int err3 = 288;
					Connection.SendAsync(new FishedItemFail(err3, 1));
					isFishing = false;
					if (FishingCancelSource != null)
					{
						FishingCancelSource.Cancel();
					}
					break;
				}
			}
		}

		public void checkActiveItem(byte last = 1)
		{
			long num2 = (LastPingTime = Utility.CurrentTimeMilliseconds());
			if (!isLogin || num2 <= LastCheckTime + 60000)
			{
				return;
			}
			LastCheckTime = num2;
			// Always run DB expire purge on the 60s tick. In-memory active items never
			// include expired rows (SP filters them), so the old loop never deleted bag items.
			ItemHandle.getActiveFuncItem(this, -1, bExpiredCheck: true);
			if (activeItem.isEmptyItems)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			foreach (NetItemInfo item in activeItem.getVector())
			{
				if (!item.m_bHasExpireTime || item.m_expireTime + 60000 > num2)
				{
					continue;
				}
				if (avatarLock.isValid && item.isCharacter)
				{
					avatarLock.clear();
				}
				hashSet.Add(item.m_iItemDescNum);
				if (eFuncItemPosition.eFuncItemPosition_FISHING_ROD == (eFuncItemPosition)item.m_position && item.m_bUsing)
				{
					isFishing = false;
					if (FishingCancelSource != null)
					{
						FishingCancelSource.Cancel();
					}
				}
			}
			if (hashSet.Count > 0)
			{
				checkEquipmentItem(hashSet.Count != 0, bDefaultAvatar: true, ref advancedAvatarInfo);
				Connection.SendAsync(new ExpiredItemInfo(hashSet.ToArray(), last));
				if (isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT(this, hashSet.ToList(), last), room.RoomServerID);
				}
			}
		}

		public void encodeUserCharAttr(PacketWriter encoder)
		{
			userItemAttr.encodeUserCharAttr(encoder);
		}

		public void encodeUserItemAttr(PacketWriter encoder)
		{
			try
			{
				mixUserItemAttr();
				mixedItemAttr.encodeUserItemAttr(encoder);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "encodeUserItemAttr failed user={0}", UserID);
				encoder.Write(0);
			}
		}

		public void mixUserItemAttr()
		{
			mixedItemAttr.clear();
			mixedItemAttr = new CUserItemAttrManager(userItemAttr);
			foreach (int key in UserItemStrengthenSlotAttr.Keys)
			{
				Dictionary<short, float> attributes = (from g in UserItemStrengthenSlotAttr[key]
					group g by g.Attr into g
					select new ItemAttr
					{
						Attr = g.Key,
						AttrValue = g.Sum((ItemAttr s) => s.AttrValue)
					}).ToDictionary((ItemAttr k) => (short)k.Attr, (ItemAttr v) => v.AttrValue);
				mixedItemAttr.addUpItemAttr(key, attributes);
			}
			foreach (KeyValuePair<int, UserEnchantItem> enchantItem in enchantItemList)
			{
				if (enchantItem.Value.getStoneAttr(out Dictionary<short, float> attr))
				{
					mixedItemAttr.addUpItemAttr(enchantItem.Value.getItemNum(), attr);
				}
			}
			charAbilityAttr.makeAttrFrom(advancedAvatarInfo, activeItem, userItemAttr, avatarLock);
			UpdateUserLuck();
		}

		public bool getAttr(short attrType, out float value)
		{
			return charAbilityAttr.m_characterAttr.m_attr.m_mapAttr.TryGetValue(attrType, out value);
		}

		private void UpdateUserLuck()
		{
			if (getAttr(9, out var value))
			{
				Luck = (decimal)value * 100m;
			}
		}

		public void getAttrs()
		{
		}

		public void setEnchantItem(int iitemNum, Dictionary<int, UserEnchantItem> enchantItemList)
		{
			if (iitemNum != -1)
			{
				this.enchantItemList.TryRemove(iitemNum, out var _);
				if (enchantItemList.Count > 0)
				{
					this.enchantItemList[iitemNum] = enchantItemList.FirstOrDefault().Value;
				}
			}
			else
			{
				this.enchantItemList = new ConcurrentDictionary<int, UserEnchantItem>(enchantItemList);
			}
			mixUserItemAttr();
		}

		public bool getEnchantItem(int iItemNum, out UserEnchantItem item)
		{
			return enchantItemList.TryGetValue(iItemNum, out item);
		}

		public void mountEnchantItem(int iitemNum, byte iSeq, byte iSocketNum, int iMountStoneNum, Dictionary<short, float> attr, out UserEnchantItem enchantItem)
		{
			enchantItem = new UserEnchantItem();
			if (enchantItemList.ContainsKey(iitemNum))
			{
				enchantItemList[iitemNum].mount(iSeq, iSocketNum, iMountStoneNum, attr);
				enchantItem = enchantItemList[iitemNum];
			}
			else
			{
				enchantItem.set(iitemNum);
				enchantItem.mount(iSeq, iSocketNum, iMountStoneNum, attr);
				enchantItemList[iitemNum] = enchantItem;
			}
			mixUserItemAttr();
		}

		public bool removeEnchantItemStone(int iitemNum, byte iSeq, out UserEnchantItem enchantItem)
		{
			enchantItem = new UserEnchantItem();
			if (enchantItemList.ContainsKey(iitemNum))
			{
				enchantItemList[iitemNum].removeStone(iSeq);
				enchantItem = enchantItemList[iitemNum];
				mixUserItemAttr();
			}
			return true;
		}

		public void removeEnchantItem(int iitemNum)
		{
			if (enchantItemList.ContainsKey(iitemNum))
			{
				Dictionary<short, float> attr = new Dictionary<short, float>();
				if (enchantItemList[iitemNum].getStoneAttr(out attr))
				{
					userItemAttr.subtractItemAttr(iitemNum, attr);
				}
				enchantItemList.TryRemove(iitemNum, out var _);
			}
		}

		public bool checkAvatar(ref AvatarInfo transAvatarInfo, ref AvatarInfo originAvatarInfo)
		{
			cpk_type cpk_type = 0;
			bool result = false;
			for (int i = 1; i < 15; i++)
			{
				ushort transKind = transAvatarInfo.GetWear(i);
				if (transKind > 20000 && transKind < 30000 && transKind != ushort.MaxValue)
				{
					CItemTransformInfo itemTransformInfo = new CItemTransformInfo();
					if (TransformItemManager.getItemTransformInfo((transAvatarInfo.GetWear(0), i, transKind), ref itemTransformInfo))
					{
						cpk_type = itemTransformInfo.m_iOriginKind;
						originAvatarInfo.SetWear(i, itemTransformInfo.m_iOriginKind);
					}
				}
				else
				{
					cpk_type = originAvatarInfo.GetWear(i);
				}
				if ((int)cpk_type != 0 && 65535 != (int)cpk_type && !ShopItemTable.getRealItemDataFromCPK(transAvatarInfo.m_character, i, cpk_type, out var _))
				{
					Log.Warning("checkAvatar CPK miss c:{0} p:{1} k:{2} - keep packed kind", (int)transAvatarInfo.m_character, i, (int)cpk_type);
				}
			}
			return result;
		}

		public void checkEquipmentItem(bool bAvatarChanged, bool bDefaultAvatar, ref AdvancedAvatarInfo advancedAvatarInfo)
		{
			if (!bAvatarChanged)
			{
				return;
			}
			AdvancedAvatarInfo advancedAvatarInfo2 = advancedAvatarInfo.ShallowCopy();
			AdvancedAvatarInfo advancedAvatarInfo3 = advancedAvatarInfo.ShallowCopy();
			bool flag = false;
			bool flag2 = false;
			if (checkAvatar(ref advancedAvatarInfo3.m_realAvatarInfo, ref advancedAvatarInfo2.m_realAvatarInfo))
			{
				flag = true;
			}
			if (checkAvatar(ref advancedAvatarInfo3.m_costumeAvatarInfo, ref advancedAvatarInfo2.m_costumeAvatarInfo))
			{
				flag2 = true;
			}
			if (flag || flag2)
			{
				MyRoomHandle.MyRoomSetCharacterSetting(UserNum, advancedAvatarInfo2, advancedAvatarInfo3, flag, flag2, bChangeAvatarMode: false);
				if (bDefaultAvatar)
				{
					setAvatarInfoAndSendTCP(advancedAvatarInfo3);
				}
			}
			advancedAvatarInfo = advancedAvatarInfo3.ShallowCopy();
		}

		public void ReplaceWornAvatar(AdvancedAvatarInfo avatarInfo)
		{
			if (avatarInfo is null)
			{
				return;
			}
			if (advancedAvatarInfo is null)
			{
				advancedAvatarInfo = new AdvancedAvatarInfo();
			}
			advancedAvatarInfo.m_realAvatarInfo = avatarInfo.m_realAvatarInfo;
			advancedAvatarInfo.m_costumeAvatarInfo = avatarInfo.m_costumeAvatarInfo;
			advancedAvatarInfo.m_bIsUseCostume = avatarInfo.m_bIsUseCostume;
			try
			{
				advancedAvatarInfo.EnsureVisibleStarterClothes();
			}
			catch (Exception ex)
			{
				Log.Warning("ReplaceWornAvatar EnsureVisible: {0}", ex.Message);
			}
		}

		public void setAvatarInfoAndSendTCP(AdvancedAvatarInfo avatarInfo, bool bRequestNickName = false, bool bSaveCharInPark = false)
		{
			if (advancedAvatarInfo is null)
			{
				advancedAvatarInfo = new AdvancedAvatarInfo();
			}
			if (avatarInfo is null)
			{
				_sendAvatarInfoOK(bRequestNickName, bSaveCharInPark);
				return;
			}
			try
			{
				CopyItemPartsUnlessUnset(ref advancedAvatarInfo.m_realAvatarInfo, ref avatarInfo.m_realAvatarInfo);
				CopyItemPartsUnlessUnset(ref advancedAvatarInfo.m_costumeAvatarInfo, ref avatarInfo.m_costumeAvatarInfo);
				advancedAvatarInfo.m_bIsUseCostume = avatarInfo.m_bIsUseCostume;
			}
			catch (Exception ex)
			{
				Log.Warning("setAvatarInfoAndSendTCP apply user={0}: {1}", UserID, ex.Message);
			}
			_sendAvatarInfoOK(bRequestNickName, bSaveCharInPark);
		}

		private static void CopyItemPartsUnlessUnset(ref AvatarInfo dest, ref AvatarInfo src)
		{
			for (byte b = 0; b < 15; b = (byte)(b + 1))
			{
				ushort value = src.GetWear(b);
				if (value == ushort.MaxValue)
				{
					continue;
				}
				if (b >= 1 && b <= 4 && value == 0)
				{
					continue;
				}
				dest.SetWear(b, value);
			}
			for (byte b = 0; b < 7; b = (byte)(b + 1))
			{
				ushort value = src.GetAcc(b);
				if (value == ushort.MaxValue)
				{
					continue;
				}
				dest.SetAcc(b, value);
			}
			if (src.GetEF() != ushort.MaxValue)
			{
				dest.SetEF(src.GetEF());
			}
		}

		private void _sendAvatarInfoOK(bool bRequestNickName, bool bSaveCharInPark)
		{
			try
			{
				EnsureAvatarDyeingSlots();
			}
			catch (Exception exSlots)
			{
				Log.Warning("_sendAvatarInfoOK dye slots: {0}", exSlots.Message);
			}
			try
			{
				FillAvatarDyeingFromWear();
			}
			catch (Exception ex2)
			{
				Log.Warning("_sendAvatarInfoOK dye fill: {0}", ex2);
			}
			try
			{
				if (Connection != null)
				{
					Connection.SendAsync(new GET_AVATAR_ACK(this, bRequestNickName, 10));
				}
			}
			catch (Exception exSend)
			{
				Log.Warning("_sendAvatarInfoOK send: {0}", exSend);
			}
			try
			{
				if (isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_UPDATE_AVATAR_INFO(this, bSaveCharInPark, 10), room.RoomServerID);
				}
			}
			catch (Exception ex3)
			{
				Log.Warning("_sendAvatarInfoOK room update: {0}", ex3.Message);
			}
		}

		private void FillAvatarDyeingFromWear()
		{
			if (advancedAvatarInfo is null)
			{
				return;
			}
			byte key = 0;
			if (ShopItemTable.isManCharacter(advancedAvatarInfo.m_realAvatarInfo.m_character))
			{
				key = 1;
			}
			else if (ShopItemTable.isWomanCharacter(advancedAvatarInfo.m_realAvatarInfo.m_character))
			{
				key = 2;
			}
			for (byte b = 1; b < 15; b = (byte)(b + 1))
			{
				if (b == 10 || b == 11)
				{
					continue;
				}
				int num2 = ((b > 10) ? (b - 2) : b);
				num2--;
				int num = ShopItemTable.getItemDescNumFromCPK(advancedAvatarInfo.m_realAvatarInfo.m_nItemPartArry[0], NetCommonFunc.getItemPositionByItemKind(b, advancedAvatarInfo.m_realAvatarInfo.m_nItemPartArry[b]), advancedAvatarInfo.m_realAvatarInfo.m_nItemPartArry[b]);
				if (num2 >= 0 && num2 < AvatarItemDyeing.Count)
				{
					AvatarItemDyeing[num2] = DyedItemList != null && DyedItemList.TryGetValue(key, num, out var value) ? value : new UserItemDyeing();
				}
			}
			for (byte b2 = 1; b2 < 15; b2 = (byte)(b2 + 1))
			{
				if (b2 == 10 || b2 == 11)
				{
					continue;
				}
				int num3 = ((b2 > 10) ? (b2 - 2) : b2);
				num3 += 11;
				int num = ShopItemTable.getItemDescNumFromCPK(advancedAvatarInfo.m_costumeAvatarInfo.m_nItemPartArry[0], NetCommonFunc.getItemPositionByItemKind(b2, advancedAvatarInfo.m_costumeAvatarInfo.m_nItemPartArry[b2]), advancedAvatarInfo.m_costumeAvatarInfo.m_nItemPartArry[b2]);
				if (num3 >= 0 && num3 < AvatarItemDyeing.Count)
				{
					AvatarItemDyeing[num3] = DyedItemList != null && DyedItemList.TryGetValue(key, num, out var value2) ? value2 : new UserItemDyeing();
				}
			}
		}

		public void onRecvActiveFuncItem(bool bOK, List<NetItemInfo> items, bool bEquipmentItem)
		{
			if (!bOK)
			{
				return;
			}
			activeItem.fromVector(items);
			List<List<NetItemInfo>> list = items.Split();
			byte b = 0;
			foreach (List<NetItemInfo> item in list)
			{
				short startindex = (short)(1630 * b);
				b = (byte)(b + 1);
				byte remainpage = (byte)(list.Count - b);
				Connection.SendAsync(new GET_ACTIVE_FUNC_ITEM_ACK(remainpage, startindex, item, 1));
			}
			if (bEquipmentItem)
			{
				checkEquipmentItem(bAvatarChanged: true, bDefaultAvatar: true, ref advancedAvatarInfo);
			}
		}

		public void onRecvActiveFuncItem(bool bOK, List<NetItemInfo> items, int iPosition)
		{
			if (!bOK)
			{
				return;
			}
			activeItem.replaceItemInfoByPosition(items, iPosition);
			List<List<NetItemInfo>> list = items.Split();
			byte b = 0;
			foreach (List<NetItemInfo> item in list)
			{
				short startindex = (short)(1630 * b);
				b = (byte)(b + 1);
				byte remainpage = (byte)(list.Count - b);
				Connection.SendAsync(new GET_ACTIVE_FUNC_ITEM_IN_POSITION_ACK(iPosition, remainpage, startindex, item, 1));
			}
		}

		public void onRecvActiveFuncItem(bool bOK, Dictionary<int, NetItemInfo> items)
		{
			if (bOK)
			{
				activeItem.replaceItemInfoByItemNum(items);
				Connection.SendAsync(new GET_ACTIVE_FUNC_ITEM_LIST_ACK(items, 1));
			}
		}

		public void modifyActiveItemForRoom()
		{
			if (isInRoom(out var room))
			{
				ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ACTIVE_ITEMS(this, 10), room.RoomServerID);
			}
		}

		public void updateItemOnOff(int iItemDescNum, bool bOnOff, out NetItemInfo offItemInfo, out NetItemInfo onItemInfo)
		{
			offItemInfo = new NetItemInfo();
			onItemInfo = new NetItemInfo();
			if (ShopItemTable.getItemDataFromItemDescNum(iItemDescNum, out var itemData))
			{
				switch (itemData.m_iOnOffType)
				{
				case 1:
					activeItem.updateItemOnOff_single(iItemDescNum, bOnOff, out offItemInfo, out onItemInfo);
					break;
				case 2:
					activeItem.updateItemOnOff_group(iItemDescNum, itemData.m_iPosition, bOnOff, out offItemInfo, out onItemInfo);
					break;
				}
			}
		}

		public void DisconnectedEvent()
		{
			if (TRADE_RESULT != null && TRADE_RESULT.Count > 0)
			{
				TRADE_RESULT tRADE_RESULT = TRADE_RESULT.OrderBy((TRADE_RESULT _) => Guid.NewGuid()).FirstOrDefault();
				if (tRADE_RESULT != null)
				{
					ItemTradeHandle.ItemTrading_Complete(UserNum, 0, tRADE_RESULT, out var _);
				}
			}
			if (CubeProcess != null)
			{
				ITEM_OPEN_INFO iTEM_OPEN_INFO = CubeProcess.result.items_open.OrderBy((ITEM_OPEN_INFO _) => Guid.NewGuid()).FirstOrDefault();
				if (iTEM_OPEN_INFO != null)
				{
					CubeHandle.itemcube_accept(this, CubeProcess.cube, iTEM_OPEN_INFO.index, 1);
				}
			}
		}
	}
}
