using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using NetMsg.Room;
using RoomServer.Holders;
using RoomServer.Packet;
using RoomServer.Packet.RoomServer;
using RoomServer.Packet.Send;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Guild;
using RoomServer.Structuring.Map;
using RoomServer.Structuring.Room;
using RoomServer.Structuring.RoundDeath;
using RoomServer.Structuring.SiegeMode;
using RoomServer.Structuring.TowerEvent;
using RoomServer.Structuring.TypingRun;
using Serilog;

namespace RoomServer.Structuring
{
	public class NormalRoom : IDisposable
	{
		public int ID;

		public string Name;

		public string Password;

		public int IsTeamPlay;

		public int ItemType;

		public bool IsStepOn;

		public int MapNum = 1;

		public int RoomKindID;

		public byte MaxPlayersCount = 8;

		public byte SlotCount = 8;

		public SortedList<byte, Account> Players = new SortedList<byte, Account>();

		public bool HasPassword;

		public List<byte> PosList = new List<byte>();

		public List<byte> RelayPosList = new List<byte>();

		public int PosWeight;

		public bool is8Player = true;

		public byte RoomMasterIndex;

		public bool isPlaying;

		public byte BuffType;

		public bool GMItem;

		public int RoomMasterAttribute;

		public bool isGoal;

		public ConcurrentDictionary<int, DateTime> KickedList = new ConcurrentDictionary<int, DateTime>();

		public ConcurrentDictionary<int, DropList> DropItem = new ConcurrentDictionary<int, DropList>();

		public List<Account> LevelUPPlayerList = new List<Account>();

		public byte Rank = 1;

		public DateTime ChangeMapTime = DateTime.Now;

		public int PlayingMapNum;

		public byte Survival;

		public int GameMode;

		public int Channel;

		public int RuleType;

		public string[] RuleType_New;

		public int PlayMode;

		public int CapsuleNum = 200;

		public ConcurrentDictionary<int, MapDrawItemInfo> RegCapsule = new ConcurrentDictionary<int, MapDrawItemInfo>();

		public float MapMaxDistance;

		public Dictionary<int, short> RewardGroupList = new Dictionary<int, short>();

		public int RoomStatus;

		public byte[] Result;

		public bool recvRandGameOver;

		public List<byte> GameOverRank = new List<byte>();

		public Dictionary<int, float> RacelengthList = new Dictionary<int, float>();

		public int CorunBossHP;

		public ConcurrentDictionary<byte, int> ClearAreaTime = new ConcurrentDictionary<byte, int>();

		public int StartKillBossRemainTime;

		public byte[] CorunModeResult;

		public List<Account> RideCattlePlayer = new List<Account>();

		public ConcurrentDictionary<long, ObjectBoss> ObjectBoss = new ConcurrentDictionary<long, ObjectBoss>();

		public ConcurrentDictionary<int, ObjectBoss> AnubisObjectBoss = new ConcurrentDictionary<int, ObjectBoss>();

		public NestedDictionary<byte, int, int> UserBounsItemInfos = new NestedDictionary<byte, int, int>();

		public Dictionary<int, BonusItemInfo> MapBonusItems = new Dictionary<int, BonusItemInfo>();

		public int ItemNum = -1;

		public long Storage_Id = -1L;

		public int isOrderBy;

		public int SendRank;

		public bool isPublic;

		public byte Round;

		public HashSet<byte> RespwanList = new HashSet<byte>();

		private Stopwatch _Stopwatch = new Stopwatch();

		private Timer _ChangeRoomMasterTimer;

		private readonly object syncRoot = new object();

		private readonly object playerLock = new object();

		private readonly object goalLock = new object();

		private readonly object gameoverLock = new object();

		private readonly object enterroomLock = new object();

		private readonly object leaveroomLock = new object();

		public readonly object bounsLock = new object();

		private readonly object SurvivalLock = new object();

		private readonly object CapsuleLock = new object();

		private readonly object RamdomGameOverLock = new object();

		private readonly Mutex enterroomMutex = new Mutex();

		public long CerremonyOfficerID;

		public int CerremonyOfficerItemNum;

		private int weddingready;

		public int FarmIndex = -1;

		public FarmRoomInfo FarmRoomInfo;

		public List<FarmMapInfo> FarmRoomMapCache;

		public FarmCraftMapData FarmCraftMapCache;

		public bool hasFishingReward;

		public int ProcessLapTime = 1;

		public bool RunlympicMode;

		private readonly object runlympicLock = new object();

		public GuildInfo GuildInfo;

		public bool IsSearchGuildMatch;

		public short GuildMatchMode;

		public int RandomSearchTime;

		public int GuildMatchRoomID;

		public int RivalGuildMatchRoomID;

		public ConcurrentDictionary<int, short> MapItem = new ConcurrentDictionary<int, short>();

		public bool RealItemAppear;

		public long RealItemLastCheckTime;

		public byte[] RealItemBuffer;

		public bool PumpkinDrop;

		public bool IsTypingRunMode;

		public List<TypingRunText> TypingRunText = new List<TypingRunText>();

		public List<TypingRunStat> TypingRunStats = new List<TypingRunStat>();

		public int BonusStageLevel;

		public Dictionary<int, GameRewardResult> BonusStageRewardInfo = new Dictionary<int, GameRewardResult>();

		private bool IsSetLapTime;

		private long EndLapTime;

		private HashSet<int> EnteredArea = new HashSet<int>();

		private byte HeroClassPos = byte.MaxValue;

		private readonly object RandomBombLock = new object();

		public bool RecvRandomBomb;

		private readonly object TeamPVPLock = new object();

		private bool SelectedTeam;

		public int HumanTeam;

		public int RedTeamPoint;

		public int BlueTeamPoint;

		public List<string> RedArea = new List<string>();

		public List<string> BlueArea = new List<string>();

		public int RoundDeath_Round;

		public bool RoundDeath_NotifyThreadStarted;

		private readonly object RoundDeathLock = new object();

		private int RoundDeath_RedTeam;

		private int RoundDeath_BlueTeam;

		private int RoundDeath_RedTeamCurrent;

		private int RoundDeath_BlueTeamCurrent;

		private List<RoundDeathStat> RoundDeath_RoundUser = new List<RoundDeathStat>();

		private readonly object RelayMatch_Lock = new object();

		public bool RelayMatch_RedEntered;

		public bool RelayMatch_BlueEntered;

		public ConcurrentDictionary<int, Tower_BoxData> TowerEvent_BoxList = new ConcurrentDictionary<int, Tower_BoxData>();

		public NormalRoom(Account User, RoomSettings settings, byte last)
		{
			Name = settings.Name;
			Password = settings.Password;
			IsTeamPlay = settings.IsTeamPlay;
			ItemType = settings.ItemType;
			IsStepOn = settings.IsStepOn;
			setMapNum(settings.MapNum);
			RoomKindID = settings.RoomKindID;
			setGameMode(User, settings.roomkindinfo);
			HasPassword = !string.IsNullOrEmpty(Password);
			if (RoomKindID == 75 || RoomKindID == 76)
			{
				FarmIndex = settings.FarmIndex;
				FarmRoomInfo = settings.FarmRoomInfo;
				User.CurrentFarmUniqueNum = FarmIndex;
			}
			Random random = new Random(Guid.NewGuid().GetHashCode());
			int num = random.Next(255);
			num += random.Next(255) << 8;
			num += random.Next(255) << 16;
			num = (ID = num + (random.Next(255) << 24));
			User.InGame = true;
			User.CurrentRoomId = num;
			if (IsTeamPlay == 2)
			{
				if (!isCompetitionEventMode())
				{
					int num2 = Players.Values.Count((Account p) => p.Team == 1);
					int num3 = Players.Values.Count((Account p) => p.Team == 2);
					User.Team = (byte)((RoomKindID == 79) ? 1u : ((num2 <= num3) ? 1u : 2u));
				}
				else
				{
					switch (User.PartyType & 1)
					{
					case 1:
						User.Team = 1;
						break;
					case 0:
						User.Team = 2;
						break;
					}
				}
			}
			if (GameMode == 3)
			{
				User.SelectRelayTeam(9);
			}
			if (PlayMode == 2)
			{
				User.IsReady = true;
			}
			User.RoomPos = (byte)((User.Attribute == 3) ? 100 : PosList.First());
			Players.Add(User.RoomPos, User);
			PosList.Remove(User.RoomPos);
			setRoomMasterIndex(User.RoomPos);
			StartAutoChangeRoomMaster();
			CheckHavingBuff(User);
			Rooms.AddRoom(ID, this);
			if (RoomKindID == 74 && ServerStatus.TowerEventEnable)
			{
				TowerEventInit();
			}
			int farmMapType;
			if (RoomKindID == 75)
			{
				FarmRoomHandle.GetFarmCraftMapDataInfo(FarmIndex, out var farmcraftmapdata);
				FarmHandle.GetFarmMapInfo(FarmIndex, 0, out var farmmapinfo, out farmMapType);
				FarmRoomMapCache = farmmapinfo;
				FarmCraftMapCache = farmcraftmapdata;
				// Always ACK craft map (empty stub OK). Skipping it left client without
				// FarmUniqueNum bind → build UI treated player as non-owner.
				User.SendAsync(new GetFarmCraftMapData(FarmIndex, farmcraftmapdata, last));
			}
			else if (RoomKindID == 76)
			{
				FarmHandle.GetFarmMapInfo(FarmIndex, User.GuildNum, out var farmmapinfo2, out farmMapType);
				FarmRoomMapCache = farmmapinfo2;
			}
			else if (RoomKindID == 79)
			{
				GuildInfo = User.GuildInfo;
				User.CurrentGuildMatchRoomId = Rooms.GuildMatchRoomID;
				GuildMatchRoomID = Rooms.GuildMatchRoomID;
				Interlocked.Increment(ref Rooms.GuildMatchRoomID);
				User.SendAsync(new GameRoom_GuildMatchRoomInfo(User, last));
			}
			User.AgentConnect.Tell(new UserEnterRoomOK
			{
				Session = User.Session,
				RoomID = User.CurrentRoomId,
				Pos = User.RoomPos,
				RoomServerID = ServerStatus.MyRoomServerID
			});
			BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(this));
			if (RoomKindID == 75 && FarmRoomInfo.isPublic)
			{
				BroadcastToAgent(new RM_To_AG_AddPublicFarmList(this));
			}
			if (isCompetitionEventMode())
			{
				BroadcastToAgent(new RM_To_AG_UpdateTeamInfo(this));
			}
		}

		public int PlayerCount()
		{
			return Players.Values.Count((Account p) => p.Attribute != 3);
		}

		public List<Account> PlayerList()
		{
			lock (playerLock)
			{
				return new List<Account>(Players.Values);
			}
		}

		public void setID(int id)
		{
			ID = id;
		}

		public void setName(string name)
		{
			Name = name;
		}

		public void setPassword(string password)
		{
			HasPassword = password != string.Empty;
			Password = password;
		}

		public void setItemType(int itemtype)
		{
			ItemType = itemtype;
		}

		public void setIsStepOn(bool isstepon)
		{
			IsStepOn = isstepon;
		}

		public void setRoomKindID(int roomkindid)
		{
			RoomKindID = roomkindid;
		}

		public void setIsTeamPlay(int isteamplay)
		{
			IsTeamPlay = isteamplay;
		}

		public void setMaxPlayersCount(byte maxplayerscount)
		{
			MaxPlayersCount = maxplayerscount;
		}

		public void setSlotCount(byte count)
		{
			SlotCount = count;
		}

		public void setPosList(byte maxplayerscount)
		{
			for (byte b = 0; b < maxplayerscount; b = (byte)(b + 1))
			{
				PosList.Add(b);
			}
		}

		public void setRelayPosWeight(byte teamslot, bool isOff)
		{
			int num = teamslot * 3 - 3;
			if (isOff)
			{
				for (int i = num; i < num + 3; i++)
				{
					PosWeight += 1 << i;
					RelayPosList.Remove((byte)(i + 3));
				}
			}
			else
			{
				for (int j = num; j < num + 3; j++)
				{
					PosWeight -= 1 << j;
					RelayPosList.Add((byte)(j + 3));
				}
			}
		}

		public void setRoomMasterIndex(byte index)
		{
			RoomMasterIndex = index;
		}

		private bool addKickedPlayer(Account User)
		{
			return KickedList.TryAdd(User.UserNum, DateTime.Now);
		}

		public void setMapNum(int mapid)
		{
			MapNum = mapid;
			ChangeMapTime = DateTime.Now;
			if (MapHolder.MapInfos.TryGetValue(mapid, out var value))
			{
				RuleType = value.RuleType;
				RuleType_New = value.RuleType_New;
			}
		}

		public void RegisterItem(int itemnum, long storage_id, int isorderby, int sendrank, bool ispublic)
		{
			ItemNum = itemnum;
			Storage_Id = storage_id;
			isOrderBy = isorderby;
			SendRank = sendrank;
			isPublic = ispublic;
		}

		public void setGameMode(Account User, RoomKindInfo roomkindinfo)
		{
			GameMode = roomkindinfo.GameMode;
			Channel = roomkindinfo.Channel;
			PlayMode = roomkindinfo.PlayMode;
			byte b = 0;
			switch (GameMode)
			{
			case 1:
				b = 8;
				is8Player = true;
				break;
			case 2:
				b = ((User.Attribute == 0) ? ServerSettingHolder.ServerSettings.SurvivalMaxUserNum : ServerSettingHolder.ServerSettings.GMRoomMaxUserNum);
				is8Player = false;
				break;
			case 3:
				b = 20;
				is8Player = false;
				setRelayPosList();
				break;
			case 5:
				b = 8;
				is8Player = true;
				break;
			case 14:
				b = 8;
				is8Player = true;
				break;
			case 16:
				b = 30;
				is8Player = false;
				break;
			case 17:
			case 18:
				b = 22;
				is8Player = false;
				break;
			case 36:
			case 37:
				b = ((RuleType == 111104) ? ServerSettingHolder.ServerSettings.competitionRelayMatchMaxUserNum : ServerSettingHolder.ServerSettings.competitionMaxUserNum);
				is8Player = false;
				break;
			case 38:
				b = 4;
				is8Player = true;
				break;
			case 32:
				b = ServerSettingHolder.ServerSettings.dungeonRaidMaxUserNum;
				is8Player = false;
				break;
			case 39:
				b = ServerSettingHolder.ServerSettings.DEV_FESTIVAL_MaxUserNum;
				is8Player = false;
				break;
			default:
				b = 8;
				is8Player = true;
				break;
			}
			if (RoomHolder.RoomKindPlayerNum.TryGetValue(RoomKindID, out var value))
			{
				b = (byte)value.MaxUser;
				is8Player = false;
			}
			setMaxPlayersCount(b);
			setPosList(b);
			setSlotCount(b);
		}

		private void setRelayPosList()
		{
			for (byte b = 3; b < 21; b = (byte)(b + 1))
			{
				RelayPosList.Add(b);
			}
		}

		public void BroadcastToAll(NetPacket np)
		{
			foreach (Account item in PlayerList())
			{
				item.SendAsync(np);
			}
		}

		public void BroadcastToAll(byte[] np)
		{
			foreach (Account item in PlayerList())
			{
				item.SendAsync(np);
			}
		}

		public void BroadcastToAgent(NetPacket packet)
		{
			foreach (IActorRef value in AgentServer.AgentServerList.Values)
			{
				value.Tell(packet.ToArray());
			}
		}

		public bool CheckCanStart(Account User, out int ErrorCode)
		{
			ErrorCode = 0;
			if (DateTime.Compare(DateTime.Now, ChangeMapTime.AddSeconds(3.0)) < 0)
			{
				ErrorCode = 9;
				return false;
			}
			if (isOrderBy == 1 && SendRank > PlayerCount())
			{
				ErrorCode = 8;
				return false;
			}
			if (!isCompetitionEventMode() && IsTeamPlay == 2)
			{
				int num = Players.Values.Count((Account p) => p.Team == 1 && p.Attribute != 3);
				int num2 = Players.Values.Count((Account p) => p.Team == 2 && p.Attribute != 3);
				if (num > num2 + 1 || num + 1 < num2)
				{
					ErrorCode = 3;
					return false;
				}
			}
			if (GameMode == 14 && HasPassword && !User.activeItem.HasPosition((ushort)800))
			{
				ErrorCode = 4;
				return false;
			}
			if (GameMode == 39)
			{
				if (!ServerSettingHolder.ServerSettings.useThankOfferingSystem)
				{
					ErrorCode = 2;
					return false;
				}
				if (!ThankOfferingSystem.ThankOfferingSchedule.TryGetValue(ServerSettingHolder.ServerSettings.ThankOfferingSchedule_CurNum, out var value))
				{
					ErrorCode = 2;
					return false;
				}
				if (!(value.StartTime <= DateTime.Now) || !(DateTime.Now <= value.EndTime))
				{
					ErrorCode = 2;
					return false;
				}
			}
			return true;
		}

		public bool CheckReadyPlayerNum(out int ErrorCode)
		{
			ErrorCode = 0;
			switch (GameMode)
			{
			case 1:
			case 14:
			case 38:
				if (!Players.Values.Where((Account p) => p.RoomPos != RoomMasterIndex).All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1 < 2)
				{
					ErrorCode = 1;
					return false;
				}
				break;
			case 2:
				if ((RuleType == 2 || RuleType == 4 || RuleType == 8) && Players.Values.Count((Account p) => p.IsReady && p.Attribute != 3) + 1 < ServerSettingHolder.ServerSettings.SurvivalMinUserNum)
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1 < 2)
				{
					ErrorCode = 1;
					return false;
				}
				break;
			case 3:
			{
				int num = Players.Values.Count((Account w) => w.RelayTeamPos > 2 && w.RelayTeamPos < 21);
				Account account = Players.Values.FirstOrDefault((Account p) => p.RoomPos == RoomMasterIndex);
				bool flag = account.Attribute == 3 || account.IsReady;
				if (num < 6 || !flag)
				{
					ErrorCode = 1;
					return false;
				}
				byte i;
				for (i = 1; i < 7; i++)
				{
					int num2 = Players.Values.Count((Account w) => w.RelayTeam == i);
					if (num2 != 0 && num2 != 3)
					{
						ErrorCode = 1;
						return false;
					}
				}
				break;
			}
			case 5:
				if (!Players.Values.Where((Account p) => p.RoomPos != RoomMasterIndex).All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1 < ServerSettingHolder.ServerSettings.corunModeMinPlayerNum)
				{
					ErrorCode = 1;
					return false;
				}
				break;
			case 32:
				if (Players.Values.Count((Account p) => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1 < ServerSettingHolder.ServerSettings.dungeonRaidMinUserNum)
				{
					ErrorCode = 1;
					return false;
				}
				break;
			case 36:
			case 37:
				if (!Players.Values.Where((Account p) => p.RoomPos != RoomMasterIndex).All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1 < ((RuleType == 111104) ? ServerSettingHolder.ServerSettings.competitionRelayMatchMinUserNum : ServerSettingHolder.ServerSettings.competitionMinUserNum))
				{
					ErrorCode = 1;
					return false;
				}
				break;
			case 39:
				if (!Players.Values.All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.Attribute != 3) < ServerSettingHolder.ServerSettings.DEV_FESTIVAL_MinUserNum)
				{
					ErrorCode = 1;
					return false;
				}
				break;
			default:
				if (!Players.Values.Where((Account p) => p.RoomPos != RoomMasterIndex).All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				break;
			}
			if (RoomHolder.RoomKindPlayerNum.TryGetValue(RoomKindID, out var value))
			{
				if (!Players.Values.All((Account player) => player.IsReady))
				{
					ErrorCode = 1;
					return false;
				}
				if (Players.Values.Count((Account p) => p.IsReady && p.Attribute != 3) < value.MinUser)
				{
					ErrorCode = 1;
					return false;
				}
			}
			if (SiegeModeHolder.SiegeModeInfos.Exists((SiegeModeInfo e) => e.MapNum == MapNum) || SiegeModeHolder.SiegeModeInfos.Exists((SiegeModeInfo e) => e.MapNum == PlayingMapNum))
			{
				int officialCompetition_ArenaMode_MinUserNum = ServerSettingHolder.ServerSettings.OfficialCompetition_ArenaMode_MinUserNum;
				if (Players.Values.Count((Account p) => p.IsReady && p.Attribute != 3) < officialCompetition_ArenaMode_MinUserNum)
				{
					ErrorCode = 1;
					return false;
				}
			}
			return true;
		}

		public void SetRewardGroupList()
		{
			RewardGroupList.Clear();
			if (GameMode != 14)
			{
				foreach (KeyValuePair<int, GameRewardGroupInfo> item in GameRewardHolder.GroupInfos.Where((KeyValuePair<int, GameRewardGroupInfo> f) => f.Value.GroupType == 0 && f.Value.Argument == -1 && (!f.Value.UsePeriod || (f.Value.UsePeriod && f.Value.StartTime <= DateTime.Now && f.Value.EndTime >= DateTime.Now && f.Value.StartHour <= DateTime.Now.Hour && f.Value.EndHour >= DateTime.Now.Hour))))
				{
					RewardGroupList.Add(item.Key, item.Value.GroupType);
				}
				foreach (KeyValuePair<int, GameRewardGroupInfo> item2 in GameRewardHolder.GroupInfos.Where((KeyValuePair<int, GameRewardGroupInfo> f) => f.Value.GroupType == 1 && f.Value.Argument == RoomKindID && (!f.Value.UsePeriod || (f.Value.UsePeriod && f.Value.StartTime <= DateTime.Now && f.Value.EndTime >= DateTime.Now && f.Value.StartHour <= DateTime.Now.Hour && f.Value.EndHour >= DateTime.Now.Hour))))
				{
					RewardGroupList.Add(item2.Key, item2.Value.GroupType);
				}
				foreach (KeyValuePair<int, GameRewardGroupInfo> type2 in GameRewardHolder.GroupInfos.Where((KeyValuePair<int, GameRewardGroupInfo> f) => f.Value.GroupType == 2 && f.Value.Argument == PlayingMapNum && (!f.Value.UsePeriod || (f.Value.UsePeriod && f.Value.StartTime <= DateTime.Now && f.Value.EndTime >= DateTime.Now && f.Value.StartHour <= DateTime.Now.Hour && f.Value.EndHour >= DateTime.Now.Hour))))
				{
					RewardGroupList.Add(type2.Key, type2.Value.GroupType);
					if (type2.Value.ChildGroupNum > 0 && !RewardGroupList.Any((KeyValuePair<int, short> a) => a.Key == type2.Value.ChildGroupNum) && GameRewardHolder.GroupInfos.TryGetValue(type2.Value.ChildGroupNum, out var value))
					{
						RewardGroupList.Add(type2.Value.ChildGroupNum, value.GroupType);
					}
				}
			}
			else
			{
				KeyValuePair<int, GameRewardGroupInfo> keyValuePair = GameRewardHolder.GroupInfos.FirstOrDefault((KeyValuePair<int, GameRewardGroupInfo> f) => f.Value.GroupType == 2 && f.Value.Argument == PlayingMapNum);
				if (keyValuePair.Value != null)
				{
					RewardGroupList.Add(keyValuePair.Key, keyValuePair.Value.GroupType);
				}
			}
			foreach (Account item3 in PlayerList())
			{
				float value2;
				bool attr = item3.getAttr(20, out value2);
				float value3;
				bool attr2 = item3.getAttr(274, out value3);
				if (attr && value2 > 15f)
				{
					value2 = ((!attr2) ? 15f : (value2 + value3));
				}
				if (RuleType == 45568 || RuleType == 111104)
				{
					item3.HP = 100;
				}
				else
				{
					item3.HP = item3.HealthPoint + item3.Level * 2 + (int)value2 * 5;
				}
				item3.MaxHP = item3.HP;
			}
		}

		public void StartGame()
		{
			_Stopwatch.Start();
		}

		public int GetCurrentTime()
		{
			return (int)_Stopwatch.ElapsedMilliseconds;
		}

		public bool isCompetitionEventMode()
		{
			switch (RoomKindID)
			{
			case 193:
			case 194:
			case 195:
			case 196:
			case 197:
			case 204:
			case 205:
			case 216:
			case 217:
			case 218:
			case 219:
			case 231:
			case 233:
			case 234:
			case 235:
				return true;
			default:
				return false;
			}
		}

		private bool EnterRoomCheck(Account User, string pw, out byte ErrorCode)
		{
			ErrorCode = 1;
			if (User.InGame || User.CurrentRoomId != 0)
			{
				ErrorCode = 8;
				return false;
			}
			if (Password != pw && User.Attribute == 0 && RoomKindID != 75)
			{
				ErrorCode = 7;
				return false;
			}
			if (Password != pw && User.Attribute == 0 && RoomKindID == 75 && User.MyFarmUniqueNum != FarmIndex)
			{
				ErrorCode = 7;
				return false;
			}
			if (isPlaying)
			{
				ErrorCode = 5;
				BroadcastToAgent(new RM_To_AG_PlayingUpdate(ID, isPlaying, 0));
				return false;
			}
			if (isCompetitionEventMode())
			{
				int num = Players.Values.Count((Account p) => p.Team == 1);
				int num2 = Players.Values.Count((Account p) => p.Team == 2);
				int num3 = (int)MaxPlayersCount / 2;
				switch (User.PartyType & 1)
				{
				case 1:
					if (num >= num3)
					{
						ErrorCode = 11;
						return false;
					}
					break;
				case 0:
					if (num2 >= num3)
					{
						ErrorCode = 11;
						return false;
					}
					break;
				}
			}
			if (PlayerCount() + 1 > ServerSettingHolder.ServerSettings.GuildMatchPartyMemberCount && RoomKindID == 79 && User.Attribute != 3)
			{
				ErrorCode = 23;
				return false;
			}
			if (PlayerCount() < SlotCount || User.Attribute == 3)
			{
				if (KickedList.TryGetValue(User.UserNum, out var value))
				{
					if (DateTime.Compare(DateTime.Now, value.AddSeconds(60.0)) < 0)
					{
						ErrorCode = 6;
						return false;
					}
					KickedList.TryRemove(User.UserNum, out var _);
				}
				if (Players.Values.Any((Account p) => p.Attribute == 3) && User.Attribute == 3)
				{
					ErrorCode = 9;
					return false;
				}
				if (MapHolder.AssaultModeLimitInfos.TryGetValue(RoomKindID, out var value3))
				{
					Log.Debug("AttackPoint: {0}", User.AttackPoint);
					if (User.AttackPoint < value3)
					{
						ErrorCode = 12;
						return false;
					}
				}
				if (Players.Values.Any((Account p) => p.UserNum == User.UserNum))
				{
					Log.Warning("Detected same account try to enter same room! userid: {0}", User.UserID);
					Account client = Players.Values.FirstOrDefault((Account p) => p.UserNum == User.UserNum);
					LeaveRoomNoLock(client, isDisconnect: true, 1);
				}
				return true;
			}
			ErrorCode = 1;
			return false;
		}

		public void EnterRoom(Account Client, string pw, byte last)
		{
			lock (enterroomLock)
			{
				enterroomMutex.WaitOne();
				if (EnterRoomCheck(Client, pw, out var ErrorCode))
				{
					Client.InGame = true;
					Client.CurrentRoomId = ID;
					Client.IsReady = false;
					Client.RoomPos = (byte)((Client.Attribute == 3) ? 100 : PosList.FirstOrDefault((byte f) => !Players.Keys.Contains(f)));
					PosList.Remove(Client.RoomPos);
					if (RoomKindID != 74 && RoomKindID != 75 && RoomKindID != 76)
					{
						Client.SendAsync(new GameRoom_GoodsInfo(this, last));
					}
					if (RoomKindID == 74 && ServerStatus.TowerEventEnable)
					{
						Client.SendAsync(new TowerEvent_TimeInfo_ACK(last));
						Client.SendAsync(new TowerEvent_RemainBox2_ACK(TowerEvent_BoxList, last));
					}
					if (RoomKindID == 75)
					{
						// Always send craft-map ACK so client binds FarmUniqueNum as owner.
						Client.SendAsync(new GetFarmCraftMapData(FarmIndex, FarmCraftMapCache ?? new Structuring.Farm.FarmCraftMapData(), last));
					}
					Client.AgentConnect.Tell(new UserEnterRoomOK
					{
						Session = Client.Session,
						RoomID = Client.CurrentRoomId,
						Pos = Client.RoomPos,
						RoomServerID = ServerStatus.MyRoomServerID
					});
					Client.SendAsync(new GameRoom_EnterRoomOK(last));
					if (RoomKindID != 75 && RoomKindID != 76)
					{
						Client.SendAsync(new GameRoom_SendRoomInfo(this, last, Client.RoomPos));
					}
					else
					{
						Client.CurrentFarmUniqueNum = FarmIndex;
						if (RoomKindID == 75)
						{
							Client.SendAsync(new SendFarmItemPos(FarmRoomMapCache, last));
						}
						Client.SendAsync(new SendFarmInfo(Client, this, FarmIndex, last));
					}
					if (RoomKindID == 79)
					{
						Client.CurrentGuildMatchRoomId = GuildMatchRoomID;
					}
					bool num = GameMode == 3;
					if (IsTeamPlay == 2)
					{
						if (!isCompetitionEventMode())
						{
							int num2 = Players.Values.Count((Account p) => p.Team == 1);
							int num3 = Players.Values.Count((Account p) => p.Team == 2);
							Client.Team = (byte)((RoomKindID == 79) ? 1u : ((num2 <= num3) ? 1u : 2u));
						}
						else if (Client.PartyType % 2 == 0)
						{
							Client.Team = 2;
						}
						else
						{
							Client.Team = 1;
						}
					}
					if (num)
					{
						int num4 = Players.Values.Count((Account p) => p.RelayTeamPos == 1);
						int num5 = Players.Values.Count((Account p) => p.RelayTeamPos == 2);
						byte relayteampos = (byte)((num4 <= num5) ? 1u : 2u);
						Client.SelectRelayTeam(relayteampos);
					}
					if (Client.Attribute == 3)
					{
						setRoomMasterIndex(100);
					}
					if (PlayMode == 2)
					{
						Client.IsReady = true;
					}
					Players.Add(Client.RoomPos, Client);
					Client.SendAsync(new GameRoom_SendPlayerInfo(Client, RoomKindID, last));
					foreach (Account item in from o in PlayerList()
						orderby o.RoomPos
						select o)
					{
						try
						{
							if (item.RoomPos != Client.RoomPos)
							{
								Client.SendAsync(new GameRoom_SendPlayerInfo(item, RoomKindID, last));
								item.SendAsync(new GameRoom_SendPlayerInfo(Client, RoomKindID, last));
							}
							if (PlayMode == 2)
							{
								item.SendAsync(new GameRoom_ChangeMap_FF0906(MapNum, last));
							}
							if (Client.Attribute == 3)
							{
								item.SendAsync(new GameRoom_GetRoomMaster(RoomMasterIndex, last));
							}
						}
						catch (Exception ex)
						{
							Log.Error("EnterRoom Error:{0}", ex.ToString());
						}
					}
					if (Client.Attribute != 3)
					{
						byte roomMasterIndex = RoomMasterIndex;
						Client.SendAsync(new GameRoom_GetRoomMaster(roomMasterIndex, last));
					}
					GetSortedHeroClassGameIndices();
					BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(this));
					if (RoomKindID == 75 && FarmRoomInfo.isPublic)
					{
						BroadcastToAgent(new RM_To_AG_AddPublicFarmList(this));
					}
					if (isCompetitionEventMode())
					{
						BroadcastToAgent(new RM_To_AG_UpdateTeamInfo(this));
					}
				}
				else
				{
					Client.SendAsync(new GameRoom_EnterRoomError(ErrorCode, RoomKindID, last));
				}
				enterroomMutex.ReleaseMutex();
			}
		}

		public void LeaveRoom(Account Client, bool isDisconnect, byte last)
		{
			Account User = Client;
			lock (enterroomLock)
			{
				lock (gameoverLock)
				{
					enterroomMutex.WaitOne();
					if (User.CurrentRoomId != 0 && User.InGame)
					{
						byte roomPos = User.RoomPos;
						bool flag = true;
						if (Players.Count == 1)
						{
							if (ItemNum != -1 && !isDisconnect)
							{
								Client.SendAsync(new GameRoom_LockKeepItem(this, isCancel: true, last));
							}
							foreach (IActorRef value2 in AgentServer.AgentServerList.Values)
							{
								value2.Tell(new RemoveRoom
								{
									RoomID = User.CurrentRoomId
								});
							}
							Rooms.RemoveRoom(User.CurrentRoomId);
						}
						else
						{
							Players.Remove(roomPos);
							if (!PosList.Contains(roomPos) && roomPos != 100)
							{
								PosList.Add(roomPos);
							}
							flag = false;
							if (isPlaying)
							{
								short gameEndType = User.GameEndType;
								DisconnectDropList(User);
								if (gameEndType == 0)
								{
									lock (SurvivalLock)
									{
										Survival--;
									}
									switch (RuleType)
									{
									case 2:
										if (Survival <= 1 && GameMode != 3)
										{
											Account account = Players.Values.FirstOrDefault((Account f) => f.GameEndType == 0 && f.Attribute != 3 && f.UserNum != User.UserNum);
											if (account != null)
											{
												BroadcastToAll(new GameRoom_Alive(account.RoomPos, last));
												account.GameEndType = 2;
												account.LapTime = GetCurrentTime();
												account.ServerLapTime = User.LapTime;
											}
											long EndTime3 = Utility.CurrentTimeMilliseconds() + 5000;
											Task.Run(delegate
											{
												GameRoomEvent.Execute_GameEnd(this, EndTime3, last);
											});
										}
										else
										{
											if (GameMode != 3)
											{
												break;
											}
											foreach (Account item in Players.Values.Where((Account w) => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
											{
												Survival--;
												item.GameEndType = 4;
												item.GameOver = true;
											}
											IEnumerable<Account> enumerable = Players.Values.Where((Account w) => w.RelayTeam != User.RelayTeam && w.GameEndType == 0);
											if (enumerable.Select((Account s) => s.RelayTeam).Distinct().Count() != 1)
											{
												break;
											}
											foreach (Account item2 in enumerable)
											{
												BroadcastToAll(new GameRoom_Alive(item2.RoomPos, last));
												item2.GameEndType = 2;
												item2.LapTime = GetCurrentTime();
												item2.ServerLapTime = User.LapTime;
											}
											long EndTime2 = Utility.CurrentTimeMilliseconds() + 5000;
											Task.Run(delegate
											{
												GameRoomEvent.Execute_GameEnd(this, EndTime2, last);
											});
										}
										break;
									case 4:
									case 8:
									case 64:
									case 256:
									case 524288:
									case 2097152:
									case 4194308:
										if (Survival == 0)
										{
											long EndTime4 = Utility.CurrentTimeMilliseconds() + 5000;
											Task.Run(delegate
											{
												GameRoomEvent.Execute_GameEnd(this, EndTime4, last);
											});
										}
										break;
									case 16384:
									case 45568:
									case 111104:
									{
										int num = PlayerList().Count((Account c) => c.Team == 1 && !c.GameOver);
										int num2 = PlayerList().Count((Account c) => c.Team == 2 && !c.GameOver);
										bool flag2 = SiegeModeHolder.SiegeModeInfos.Exists((SiegeModeInfo e) => e.MapNum == PlayingMapNum);
										bool flag3 = RuleType_New.Contains("rumble_garden");
										bool flag4 = RuleType_New.Contains("round_death");
										if (!flag2 && !flag4)
										{
											User.GameEndType = 4;
										}
										if ((num != 0 && num2 != 0) || flag2 || flag4 || flag3)
										{
											break;
										}
										foreach (Account item3 in from w in PlayerList()
											where !w.GameOver
											select w)
										{
											item3.LapTime = GetCurrentTime();
											item3.ServerLapTime = GetCurrentTime();
										}
										long EndTime = Utility.CurrentTimeMilliseconds() + 100;
										Task.Run(delegate
										{
											GameRoomEvent.Execute_GameEnd(this, EndTime, last);
										});
										break;
									}
									default:
										if (RuleType_New.Contains("hardcore") && Survival == 0)
										{
											long EndTime5 = Utility.CurrentTimeMilliseconds() + 5000;
											Task.Run(delegate
											{
												GameRoomEvent.Execute_GameEnd(this, EndTime5, last);
											});
										}
										break;
									}
								}
							}
						}
						if (IsSearchGuildMatch)
						{
							IsSearchGuildMatch = false;
							RandomSearchTime = 0;
							if (!flag)
							{
								BroadcastToAll(new GameRoom_CancelGuildMatching(last));
							}
						}
						if (!isDisconnect)
						{
							if (RoomKindID == 74 && ServerStatus.TowerEventEnable && User.TowerEventJoined)
							{
								User.TowerEventJoined = false;
								Client.SendAsync(new TowerEvent_GiveUP_ACK(last));
							}
							User.LeaveRoomReset();
							Client.SendAsync(new GameRoom_LeaveRoomUser_0XA9(roomPos, last));
							Client.SendAsync(new GameRoom_UnknownResponse2(last));
							User.AgentConnect.Tell(new UserLeaveRoomOK
							{
								Session = User.Session
							});
						}
						NormalRoom value;
						if (!flag)
						{
							BroadcastToAll(new GameRoom_RemoveRoomUser(roomPos, last));
							if (roomPos == RoomMasterIndex)
							{
								if (ItemNum != -1)
								{
									if (!isDisconnect)
									{
										Client.SendAsync(new GameRoom_LockKeepItem(this, isCancel: true, last));
									}
									RegisterItem(-1, -1L, 0, 0, ispublic: false);
									BroadcastToAll(new GameRoom_GoodsInfo(this, last));
								}
								Account account2 = (Players.Values.Any((Account p) => p.Attribute == 1) ? Players.Values.FirstOrDefault((Account p) => p.Attribute == 1) : Players.Values.FirstOrDefault());
								RoomMasterIndex = account2.RoomPos;
								CheckHavingBuff(account2);
								BroadcastToAll(new GameRoom_GetRoomMaster(account2.RoomPos, last));
							}
							GetSortedHeroClassGameIndices();
							if (RoomKindID == 75 && User.MyFarmUniqueNum == FarmIndex && FarmRoomInfo.isPublic)
							{
								foreach (IActorRef value3 in AgentServer.AgentServerList.Values)
								{
									value3.Tell(new RemoveFarmRoom
									{
										RoomID = ID
									});
								}
								Rooms.PublicFarmRoom.TryRemove(ID, out value);
								FarmRoomInfo.isPublic = false;
								setSlotCount(20);
								BroadcastToAll(new PublicFarmOpen_Ack(FarmRoomInfo, 1));
							}
							BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(this));
							if (RoomKindID == 75 && FarmRoomInfo.isPublic)
							{
								BroadcastToAgent(new RM_To_AG_AddPublicFarmList(this));
							}
							if (isCompetitionEventMode())
							{
								BroadcastToAgent(new RM_To_AG_UpdateTeamInfo(this));
							}
						}
						else
						{
							if (RoomKindID == 75 || RoomKindID == 76)
							{
								Rooms.PublicFarmRoom.TryRemove(ID, out value);
							}
							foreach (IActorRef value4 in AgentServer.AgentServerList.Values)
							{
								value4.Tell(new RemoveFarmRoom
								{
									RoomID = ID
								});
							}
							Dispose();
						}
						if (User.isFishing)
						{
							User.isFishing = false;
							if (User.FishingCancelSource != null)
							{
								User.FishingCancelSource.Cancel();
							}
						}
					}
					enterroomMutex.ReleaseMutex();
				}
			}
		}

		public void LeaveRoomNoLock(Account Client, bool isDisconnect, byte last)
		{
			if (Client.CurrentRoomId == 0 || !Client.InGame)
			{
				return;
			}
			byte roomPos = Client.RoomPos;
			bool flag = true;
			if (Players.Count == 1)
			{
				foreach (IActorRef value2 in AgentServer.AgentServerList.Values)
				{
					value2.Tell(new RemoveRoom
					{
						RoomID = Client.CurrentRoomId
					});
				}
				Rooms.RemoveRoom(Client.CurrentRoomId);
			}
			else
			{
				Players.Remove(roomPos);
				if (!PosList.Contains(roomPos) && roomPos != 100)
				{
					PosList.Add(roomPos);
				}
				flag = false;
			}
			if (IsSearchGuildMatch)
			{
				IsSearchGuildMatch = false;
				RandomSearchTime = 0;
				if (!flag)
				{
					BroadcastToAll(new GameRoom_CancelGuildMatching(last));
				}
			}
			NormalRoom value;
			if (!flag)
			{
				BroadcastToAll(new GameRoom_RemoveRoomUser(roomPos, last));
				if (roomPos == RoomMasterIndex)
				{
					if (ItemNum != -1)
					{
						if (!isDisconnect)
						{
							Client.SendAsync(new GameRoom_LockKeepItem(this, isCancel: true, last));
						}
						RegisterItem(-1, -1L, 0, 0, ispublic: false);
						BroadcastToAll(new GameRoom_GoodsInfo(this, last));
					}
					Account account = (Players.Values.Any((Account p) => p.Attribute == 1) ? Players.Values.FirstOrDefault((Account p) => p.Attribute == 1) : Players.Values.FirstOrDefault());
					RoomMasterIndex = account.RoomPos;
					CheckHavingBuff(account);
					BroadcastToAll(new GameRoom_GetRoomMaster(account.RoomPos, last));
				}
				GetSortedHeroClassGameIndices();
				if (RoomKindID == 75 && Client.MyFarmUniqueNum == FarmIndex && FarmRoomInfo.isPublic)
				{
					foreach (IActorRef value3 in AgentServer.AgentServerList.Values)
					{
						value3.Tell(new RemoveFarmRoom
						{
							RoomID = ID
						});
					}
					Rooms.PublicFarmRoom.TryRemove(ID, out value);
					FarmRoomInfo.isPublic = false;
					setSlotCount(20);
					BroadcastToAll(new PublicFarmOpen_Ack(FarmRoomInfo, 1));
				}
				BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(this));
				if (RoomKindID == 75 && FarmRoomInfo.isPublic)
				{
					BroadcastToAgent(new RM_To_AG_AddPublicFarmList(this));
				}
				if (isCompetitionEventMode())
				{
					BroadcastToAgent(new RM_To_AG_UpdateTeamInfo(this));
				}
			}
			else
			{
				if (RoomKindID == 75 || RoomKindID == 76)
				{
					Rooms.PublicFarmRoom.TryRemove(ID, out value);
				}
				foreach (IActorRef value4 in AgentServer.AgentServerList.Values)
				{
					value4.Tell(new RemoveFarmRoom
					{
						RoomID = ID
					});
				}
				Dispose();
			}
			if (Client.isFishing)
			{
				Client.isFishing = false;
				if (Client.FishingCancelSource != null)
				{
					Client.FishingCancelSource.Cancel();
				}
			}
		}

		public bool SlotControl(byte roompos, bool isOff)
		{
			lock (enterroomLock)
			{
				if (isOff)
				{
					if (SlotCount == 2)
					{
						return false;
					}
					PosWeight += 1 << (int)roompos;
					PosList.Remove(roompos);
					SlotCount--;
					return true;
				}
				if (SlotCount == 8)
				{
					return false;
				}
				PosWeight -= 1 << (int)roompos;
				PosList.Add(roompos);
				SlotCount++;
				return true;
			}
		}

		public void PressStartGame(Account Client, int mapNum, byte last)
		{
			lock (enterroomLock)
			{
				bool flag = Client.RoomPos == RoomMasterIndex;
				if (!CheckReadyPlayerNum(out var ErrorCode))
				{
					if (ErrorCode > 0)
					{
						Client.SendAsync(new GameRoom_StartError(ErrorCode, last));
					}
					return;
				}
				int ErrorCode2;
				bool flag2 = CheckCanStart(Client, out ErrorCode2);
				if (flag && flag2)
				{
					MapNum = mapNum;
					isPlaying = true;
					BroadcastToAgent(new RM_To_AG_PlayingUpdate(ID, isPlaying: true, BonusStageLevel));
					BroadcastToAll(new GameRoom_StartGame2(last));
					if (!is8Player)
					{
						StartTimeoutCountDownThread();
					}
				}
				else if (flag && !flag2)
				{
					if (ErrorCode2 == 1 || ErrorCode2 == 2 || ErrorCode2 == 3)
					{
						Client.SendAsync(new GameRoom_CannotStart((byte)ErrorCode2, last));
					}
					else if (ErrorCode2 == 4)
					{
						Client.SendAsync(new GameRoom_CancelCountDown(last));
						Client.SendAsync(new GameRoom_CannotStart((byte)ErrorCode2, last));
					}
					else if (ErrorCode2 >= 8)
					{
						Client.SendAsync(new GameRoom_StartError(ErrorCode2, last));
					}
					else
					{
						Client.SendAsync(new GameRoom_CannotStart((byte)ErrorCode2, last));
					}
				}
			}
		}

		public void GameOver(Account User, int type, byte last)
		{
			lock (enterroomLock)
			{
				lock (gameoverLock)
				{
					if (User.GameEndType != 0 || User.GameOver)
					{
						return;
					}
					lock (SurvivalLock)
					{
						Survival--;
					}
					User.GameOver = true;
					BroadcastToAll(new GameRoom_GameOver(User.RoomPos, type, last));
					switch (RuleType)
					{
					case 2:
						if (Survival <= 1 && GameMode != 3)
						{
							Account account = Players.Values.FirstOrDefault((Account f) => f.GameEndType == 0 && !f.GameOver && f.Attribute != 3 && f.UserNum != User.UserNum);
							if (account != null)
							{
								BroadcastToAll(new GameRoom_Alive(account.RoomPos, last));
								account.GameEndType = 2;
								account.LapTime = GetCurrentTime();
								account.ServerLapTime = User.LapTime;
							}
							long EndTime2 = Utility.CurrentTimeMilliseconds() + 5000;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime2, last);
							});
						}
						else
						{
							if (GameMode != 3)
							{
								return;
							}
							foreach (Account item in Players.Values.Where((Account w) => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
							{
								Survival--;
								item.GameEndType = 4;
								item.GameOver = true;
							}
							IEnumerable<Account> enumerable = Players.Values.Where((Account w) => w.RelayTeam != User.RelayTeam && w.GameEndType == 0);
							if (enumerable.Select((Account s) => s.RelayTeam).Distinct().Count() != 1)
							{
								return;
							}
							foreach (Account item2 in enumerable)
							{
								BroadcastToAll(new GameRoom_Alive(item2.RoomPos, last));
								item2.GameEndType = 2;
								item2.LapTime = GetCurrentTime();
								item2.ServerLapTime = User.LapTime;
							}
							long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime, last);
							});
						}
						return;
					case 1:
					case 4:
					case 8:
					case 64:
					case 256:
					case 524288:
					case 2097152:
					case 4194308:
						if (Survival == 0)
						{
							long EndTime3 = Utility.CurrentTimeMilliseconds() + 5000;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime3, last);
							});
						}
						return;
					}
					if (RuleType_New.Contains("hardcore") && Survival == 0)
					{
						long EndTime4 = Utility.CurrentTimeMilliseconds() + 5000;
						Task.Run(delegate
						{
							GameRoomEvent.Execute_GameEnd(this, EndTime4, last);
						});
					}
				}
			}
		}

		public void TimeOver(Account User, byte last)
		{
			lock (enterroomLock)
			{
				lock (gameoverLock)
				{
					if (User.GameEndType != 0 || User.GameOver)
					{
						return;
					}
					lock (SurvivalLock)
					{
						Survival--;
					}
					User.GameOver = true;
					int ruleType = RuleType;
					if (ruleType == 45568 || ruleType == 111104)
					{
						if (Survival == 0)
						{
							long EndTime2 = Utility.CurrentTimeMilliseconds() + 100;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime2, last);
							});
						}
					}
					else if (Survival == 0)
					{
						long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
						Task.Run(delegate
						{
							GameRoomEvent.Execute_GameEnd(this, EndTime, last);
						});
					}
				}
			}
		}

		public void GoalIn(Account User, int laptime, int servertime, byte last)
		{
			lock (goalLock)
			{
				User.LapTime = laptime;
				User.ServerLapTime = servertime;
				if (!isGoal)
				{
					MapHolder.MapInfos.TryGetValue(PlayingMapNum, out var value);
					if (GameMode == 5 && CorunBossHP > 0)
					{
						return;
					}
					if (value.GoalInLimitTime * 1000 < servertime)
					{
						isGoal = true;
						foreach (Account item in PlayerList())
						{
							item.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
							if (!RunlympicMode)
							{
								item.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last));
							}
						}
						if (GameMode == 5)
						{
							int num = 0;
							foreach (Account item2 in Players.Values.OrderBy((Account o) => o.RoomPos))
							{
								item2.LapTime = laptime + num;
								item2.ServerLapTime = servertime + num;
								num++;
							}
						}
						if (RuleType == 256)
						{
							foreach (Account item3 in from w in Players.Values
								where w.GameEndType == 0
								select w into o
								orderby o.RoomPos
								select o)
							{
								item3.LapTime = laptime;
								item3.ServerLapTime = servertime;
								item3.GameEndType = 1;
							}
						}
						if (RuleType == 16384)
						{
							User.GameEndType = 1;
							foreach (Account item4 in from w in PlayerList()
								where !w.GameOver
								select w)
							{
								item4.LapTime = GetCurrentTime();
								item4.ServerLapTime = GetCurrentTime();
							}
							long EndTime2 = Utility.CurrentTimeMilliseconds() + 1000;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime2, last);
							});
						}
						if (RuleType == 45568 || RuleType == 111104)
						{
							User.GameEndType = 1;
						}
						if (RuleType != 16384)
						{
							long EndTime = Utility.CurrentTimeMilliseconds() + 15000;
							Task.Run(delegate
							{
								GameRoomEvent.Execute_GameEnd(this, EndTime, last);
							});
						}
					}
					else
					{
						Log.Warning("Player [{0}] {1}s goal in {2} map too fast!", User.NickName, servertime / 1000, PlayingMapNum);
						GameRoomEvent.HackingBan(User);
						User.SendAsync(new DisconnectPacket(User, 258, last));
						User.isDisconnected = true;
					}
				}
				else
				{
					BroadcastToAll(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
				}
			}
		}

		public void KickPlayer(Account KickedPlayer, byte last)
		{
			try
			{
				lock (enterroomLock)
				{
					if (KickedPlayer.Attribute == 0 && !addKickedPlayer(KickedPlayer))
					{
						return;
					}
					byte roomPos = KickedPlayer.RoomPos;
					BroadcastToAll(new GameRoom_KickPlayer(roomPos, last));
					Players.Remove(roomPos);
					PosList.Add(roomPos);
					KickedPlayer.LeaveRoomReset();
					KickedPlayer.AgentConnect.Tell(new UserLeaveRoomOK
					{
						Session = KickedPlayer.Session
					});
					KickedPlayer.SendAsync(new GameRoom_KickPlayer2(last));
					KickedPlayer.SendAsync(new GameRoom_LeaveRoomUser_0XA9(roomPos, last));
					KickedPlayer.SendAsync(new GameRoom_UnknownResponse2(last));
					KickedPlayer.SendAsync(new GameRoom_KickPlayer(roomPos, last));
					if (Players.Count == 0)
					{
						foreach (IActorRef value in AgentServer.AgentServerList.Values)
						{
							value.Tell(new RemoveRoom
							{
								RoomID = KickedPlayer.CurrentRoomId
							});
						}
						Rooms.RemoveRoom(ID);
						return;
					}
					if (IsSearchGuildMatch)
					{
						IsSearchGuildMatch = false;
						RandomSearchTime = 0;
						BroadcastToAll(new GameRoom_CancelGuildMatching(last));
					}
					BroadcastToAll(new GameRoom_RemoveRoomUser(roomPos, last));
					GetSortedHeroClassGameIndices();
					BroadcastToAll(new GameRoom_UnknownResponse2(last));
					BroadcastToAgent(new RM_To_AG_CreateRoom_AddList_ACK(this));
				}
			}
			catch (Exception ex)
			{
				Log.Error("KickPlayer error:{0}", ex.Message);
			}
		}

		public void DisconnectDropList(Account User)
		{
			DropList value;
			bool num = DropItem.TryGetValue(User.UserNum, out value);
			User.GameEndType = 4;
			User.GameOver = true;
			if (!num)
			{
				DropList value2 = new DropList
				{
					UserNum = User.UserNum,
					NickName = User.NickName,
					FreePassType = 0,
					TotalEXP = User.Exp,
					RaceDistance = 0f,
					ServerLapTime = 1000000000,
					LapTime = 1000000000,
					Pos = User.RoomPos,
					Team = User.Team,
					RelayTeam = User.RelayTeam,
					RelayTeamPos = User.RelayTeamPos,
					BounsTR = 0,
					BounsEXP = 0,
					TR = 0,
					EXP = 0,
					MiniGamePoint = 0,
					MiniGameStarPoint = 0,
					TotalDamage = 0,
					MaxDamage = 0,
					Rank = 99,
					isLevelUP = false,
					CardID = new List<int> { 0 },
					GuildInfo = User.GuildInfo
				};
				DropItem.TryAdd(User.UserNum, value2);
			}
			else
			{
				value.FreePassType = 0;
				value.RaceDistance = 0f;
				value.ServerLapTime = 1000000000;
				value.LapTime = 1000000000;
				value.BounsTR = 0;
				value.BounsEXP = 0;
				value.TR = 0;
				value.EXP = 0;
				value.MiniGamePoint = 0;
				value.MiniGameStarPoint = 0;
				value.TotalDamage = 0;
				value.MaxDamage = 0;
				value.Rank = 99;
			}
			if (GameMode == 3)
			{
				foreach (Account item in Players.Values.Where((Account w) => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
				{
					if (!DropItem.TryGetValue(item.UserNum, out var value3))
					{
						DropList value4 = new DropList
						{
							UserNum = item.UserNum,
							NickName = item.NickName,
							FreePassType = 0,
							TotalEXP = item.Exp,
							RaceDistance = 0f,
							ServerLapTime = 1000000000,
							LapTime = 1000000000,
							Pos = item.RoomPos,
							Team = item.Team,
							RelayTeam = item.RelayTeam,
							RelayTeamPos = item.RelayTeamPos,
							BounsTR = 0,
							BounsEXP = 0,
							TR = 0,
							EXP = 0,
							MiniGamePoint = 0,
							MiniGameStarPoint = 0,
							Rank = 99,
							isLevelUP = false,
							CardID = new List<int> { 0 },
							GuildInfo = item.GuildInfo
						};
						DropItem.TryAdd(item.UserNum, value4);
					}
					else
					{
						value3.FreePassType = 0;
						value3.RaceDistance = 0f;
						value3.ServerLapTime = 1000000000;
						value3.LapTime = 1000000000;
						value3.BounsTR = 0;
						value3.BounsEXP = 0;
						value3.TR = 0;
						value3.EXP = 0;
						value3.MiniGamePoint = 0;
						value3.MiniGameStarPoint = 0;
						value3.Rank = 99;
					}
					item.GameEndType = 4;
				}
			}
			if (RunlympicMode)
			{
				value.Rank = (byte)(PlayerCount() + 1);
			}
		}

		public void GameEndSetNewRoomMaster()
		{
			if (Players.Values.Any((Account p) => p.Attribute == 3) || GameMode == 14 || HasPassword || GameMode == 36 || GameMode == 37 || CheckHavingKeepRoomMaster() || PlayerList().Exists((Account p) => p.Attribute != 0))
			{
				return;
			}
			Account account = (PlayerList().Exists((Account p) => p.Attribute == 1) ? PlayerList().FirstOrDefault((Account p) => p.Attribute == 1) : (from p in PlayerList()
				orderby p.Rank, p.RaceDistance descending, p.ServerLapTime
				select p).ThenBy((Account o) => o.RoomPos).FirstOrDefault());
			if (account == null)
			{
				account = (from _ in PlayerList()
					orderby Guid.NewGuid()
					select _).FirstOrDefault();
			}
			RoomMasterIndex = account.RoomPos;
			CheckHavingBuff(account);
			BroadcastToAll(new GameRoom_GetRoomMaster(account.RoomPos, 1));
		}

		public void GameRoomResult()
		{
			int num = 0;
			DropList dropList = (from p in DropItem.Values
				orderby p.Rank, p.ServerLapTime
				select p).FirstOrDefault();
			if (dropList != null)
			{
				num = dropList.ServerLapTime;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_gameresultRoom";
				mySqlCommand.Parameters.Add("mapnum", MySqlDbType.Int32).Value = PlayingMapNum;
				mySqlCommand.Parameters.Add("ptime", MySqlDbType.Int32).Value = num;
				mySqlCommand.Parameters.Add("totalExp", MySqlDbType.Int32).Value = DropItem.Values.Sum((DropList s) => s.EXP);
				mySqlCommand.Parameters.Add("totalTR", MySqlDbType.Int32).Value = DropItem.Values.Sum((DropList s) => s.TR);
				mySqlCommand.Parameters.Add("playerCount", MySqlDbType.Int32).Value = DropItem.Count;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_gameresultRoom Error: {0}", ex.ToString());
			}
		}

		public void GameEndReset()
		{
			Rank = 1;
			Result = null;
			DropItem.Clear();
			LevelUPPlayerList.Clear();
			RespwanList.Clear();
			isGoal = false;
			if (GameMode != 39)
			{
				isPlaying = false;
			}
			PlayingMapNum = 0;
			RuleType = 0;
			RoomStatus = 0;
			ProcessLapTime = 1;
			RunlympicMode = false;
			RealItemAppear = false;
			PumpkinDrop = false;
			IsSetLapTime = false;
			EnteredArea.Clear();
			foreach (Account value in Players.Values)
			{
				value.IsReady = false;
				value.EndLoading = false;
				value.GameEndType = 0;
				value.GameOver = false;
				value.Partner = 8;
				value.Fatigue = 0f;
				value.TeamLeader = false;
				value.RequestChange = false;
				value.TotalDamage = 0;
				value.MaxDamage = 0;
				value.RebirthTime = 0;
				value.RaceDistance = 0f;
				value.LapinTime.Clear();
				value.RunlympicPoint = 0;
				value.RealItem = false;
				value.GetPumpkin = false;
				value.BonusStagePoint = 0;
				value.BonusStageExtraPoint = 0;
				value.TypingRunIndex = 0;
				value.HasBomb = false;
				value.BombEndTime = 0L;
				value.BonusItemMade.Clear();
			}
			RegCapsule.Clear();
			_Stopwatch.Reset();
			ClearAreaTime.Clear();
			ObjectBoss.Clear();
			AnubisObjectBoss.Clear();
			UserBounsItemInfos.Clear();
			MapBonusItems.Clear();
			RideCattlePlayer.Clear();
			IsTypingRunMode = false;
			TypingRunText.Clear();
			TypingRunStats.Clear();
			BonusStageLevel = 0;
			BonusStageRewardInfo.Clear();
			SelectedTeam = false;
			HumanTeam = 0;
			RedTeamPoint = 0;
			BlueTeamPoint = 0;
			RedArea.Clear();
			BlueArea.Clear();
			RoundDeath_Round = 0;
			RoundDeath_RedTeam = 0;
			RoundDeath_BlueTeam = 0;
			RoundDeath_RedTeamCurrent = 0;
			RoundDeath_BlueTeamCurrent = 0;
			RoundDeath_RoundUser.Clear();
			RelayMatch_RedEntered = false;
			RelayMatch_BlueEntered = false;
			if (GameMode != 39)
			{
				BroadcastToAgent(new RM_To_AG_PlayingUpdate(ID, isPlaying: false, 0));
			}
		}

		private bool isNotResetAutoChangeRoomMasterMode()
		{
			int gameMode = GameMode;
			if ((uint)(gameMode - 39) <= 1u)
			{
				return true;
			}
			return false;
		}

		public void StartAutoChangeRoomMaster()
		{
			if (!isNotResetAutoChangeRoomMasterMode())
			{
				_ChangeRoomMasterTimer = new Timer(delegate
				{
					ChangeRoomMaster();
				}, null, TimeSpan.FromMinutes(3.0), TimeSpan.FromMinutes(3.0));
			}
			else
			{
				_ChangeRoomMasterTimer = new Timer(delegate
				{
					ChangeRoomMaster();
				}, null, TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0));
			}
		}

		public void ResetAutoChangeRoomMaster()
		{
			try
			{
				if (!isNotResetAutoChangeRoomMasterMode())
				{
					_ChangeRoomMasterTimer.Change(TimeSpan.FromMinutes(3.0), TimeSpan.FromMinutes(3.0));
				}
			}
			catch (ObjectDisposedException)
			{
				StartAutoChangeRoomMaster();
			}
		}

		private void ChangeRoomMaster()
		{
			lock (enterroomLock)
			{
				if (RoomMasterAttribute != 0 || Players.Count <= 1 || isPlaying || RoomKindID == 75 || GameMode == 14 || HasPassword)
				{
					return;
				}
				Account account;
				if (PlayerList().Any((Account p) => p.Attribute == 1))
				{
					account = PlayerList().FirstOrDefault((Account p) => p.Attribute == 1);
				}
				else
				{
					account = PlayerList().FirstOrDefault((Account f) => f.RoomPos > RoomMasterIndex);
					if (account == null)
					{
						account = (from o in PlayerList()
							orderby o.RoomPos
							select o).FirstOrDefault();
					}
				}
				if (account.RoomPos != RoomMasterIndex && ItemNum != -1)
				{
					PlayerList().FirstOrDefault((Account f) => f.RoomPos == RoomMasterIndex).SendAsync(new GameRoom_LockKeepItem(this, isCancel: true, 1));
					RegisterItem(-1, -1L, 0, 0, ispublic: false);
					BroadcastToAll(new GameRoom_GoodsInfo(this, 1));
				}
				RoomMasterIndex = account.RoomPos;
				CheckHavingBuff(account);
				BroadcastToAll(new GameRoom_GetRoomMaster(account.RoomPos, 1));
			}
		}

		public void StartWaitAllSyncThread()
		{
			Task.Run(delegate
			{
				WaitAllSync();
			});
		}

		private async void WaitAllSync()
		{
			long LoadingTimeOut = Utility.CurrentTimeMilliseconds() + ServerSettingHolder.ServerSettings.LoadingTimeOutMilliSeconds;
			bool isAllSync = false;
			if (PlayMode != 2)
			{
				_ChangeRoomMasterTimer.Dispose();
			}
			RoomStatus = 1;
			while (Utility.CurrentTimeMilliseconds() < LoadingTimeOut)
			{
				await Task.Delay(2000);
				if (Players.Values.All((Account p) => p.EndLoading))
				{
					await Task.Delay(500);
					BroadcastToAll(new GameRoom_AllSync(1));
					isAllSync = true;
					break;
				}
			}
			if (!isAllSync)
			{
				foreach (Account item in from player in PlayerList()
					where !player.EndLoading
					select player)
				{
					KickPlayer(item, 1);
					if (item.RoomPos == RoomMasterIndex)
					{
						if (ItemNum != -1)
						{
							item.SendAsync(new GameRoom_LockKeepItem(this, isCancel: true, 1));
							RegisterItem(-1, -1L, 0, 0, ispublic: false);
						}
						if (PlayerList().Count > 0)
						{
							Account account = PlayerList().FirstOrDefault();
							RoomMasterIndex = account.RoomPos;
							BroadcastToAll(new GameRoom_GetRoomMaster(account.RoomPos, 1));
						}
					}
					Survival--;
				}
				if (Players.Count == 0)
				{
					foreach (IActorRef value in AgentServer.AgentServerList.Values)
					{
						value.Tell(new RemoveRoom
						{
							RoomID = ID
						});
					}
					Rooms.RemoveRoom(ID);
					Dispose();
				}
				else
				{
					BroadcastToAll(new GameRoom_AllSync(1));
				}
			}
			RoomStatus = 2;
		}

		public void StartTimeoutCountDownThread()
		{
			Task.Run(delegate
			{
				TimeoutCountDown();
			});
		}

		private async void TimeoutCountDown()
		{
			long nowtime = Utility.CurrentTimeMilliseconds();
			while (Utility.CurrentTimeMilliseconds() < nowtime + 10000)
			{
				await Task.Delay(1000);
				if (PlayingMapNum != 0 || !isPlaying)
				{
					break;
				}
			}
			if (PlayingMapNum == 0 || !isPlaying)
			{
				BroadcastToAll(new GameRoom_CancelCountDown(1));
				isPlaying = false;
				BroadcastToAgent(new RM_To_AG_PlayingUpdate(ID, isPlaying: false, 0));
			}
		}

		public void StartHareAndTortoiseThread()
		{
			Task.Run(delegate
			{
				HareAndTortoiseStatus();
			});
		}

		private async void HareAndTortoiseStatus()
		{
			int start = 0;
			while (isPlaying && PlayingMapNum == 1606)
			{
				start++;
				await Task.Delay(1000);
				bool flag = start % 4 == 0;
				foreach (Account value in Players.Values)
				{
					float fatigue = value.Fatigue;
					if (value.TeamLeader)
					{
						if (fatigue <= 97f)
						{
							value.Fatigue += ServerSettingHolder.ServerSettings.RABBIT_TURTLE_FATIGUE_INC;
						}
						else
						{
							value.Fatigue = 100f;
						}
					}
					else if (fatigue >= 3f)
					{
						value.Fatigue -= ServerSettingHolder.ServerSettings.RABBIT_TURTLE_FATIGUE_DEC;
					}
					else
					{
						value.Fatigue = 0f;
					}
					if (flag)
					{
						value.SendAsync(new TeamStatus(value, this, 1));
					}
				}
			}
		}

		public void CheckHavingBuff(Account roomMaster)
		{
			try
			{
				if (roomMaster != null)
				{
					byte buffType = 0;
					if (roomMaster.activeItem.IsItemON((ushort)101, 2))
					{
						buffType = 1;
					}
					else if (roomMaster.activeItem.IsItemON((ushort)101, 3))
					{
						buffType = 2;
					}
					BuffType = buffType;
					RoomMasterAttribute = roomMaster.Attribute;
					GMItem = roomMaster.activeItem.IsItemON(2970);
				}
				else
				{
					BuffType = 0;
					RoomMasterAttribute = 0;
					GMItem = false;
				}
			}
			catch (Exception ex)
			{
				BuffType = 0;
				RoomMasterAttribute = 0;
				GMItem = false;
				Log.Error("CheckHavingBuff Error:{0}", ex.ToString());
			}
		}

		private bool CheckHavingKeepRoomMaster()
		{
			bool result = false;
			Account account = PlayerList().FirstOrDefault((Account p) => p.RoomPos == RoomMasterIndex);
			if (account == null)
			{
				return false;
			}
			if (account.activeItem.IsItemON((ushort)101, 2))
			{
				result = true;
			}
			if (account.activeItem.IsItemON((ushort)101, 3))
			{
				result = true;
			}
			if (account.activeItem.IsItemON((ushort)101, 1))
			{
				result = true;
			}
			return result;
		}

		public void Wedding(Account User)
		{
			lock (syncRoot)
			{
				weddingready++;
				if (weddingready == 1)
				{
					User.SendAsync(new WeddingReadyACK(1));
				}
				else
				{
					if (weddingready != 2)
					{
						return;
					}
					Account account = PlayerList().FirstOrDefault((Account f) => f.NickName == FarmRoomInfo.MasterName);
					if (account != null)
					{
						if (CoupleHandle.WeddingCreateCouple(account, CerremonyOfficerID, CerremonyOfficerItemNum))
						{
							BroadcastToAll(new WeddingFinishACK(CerremonyOfficerID, 1));
							BroadcastToAll(new ModifyObjectValueInfoOK(CerremonyOfficerID, 1, 1));
						}
					}
					else
					{
						Log.Error("Wedding FarmMaster is null");
					}
					weddingready = 0;
				}
			}
		}

		public void StartRunlympicThread()
		{
		}

		public void TowerEventInit()
		{
			if (RoomKindID != 74)
			{
				return;
			}
			TowerEvent_BoxList.Clear();
			TowerEventHolder.TowerQuizList.TryGetValue(3, out var value);
			for (int i = 1; i <= 300; i++)
			{
				if (i <= 200)
				{
					Tower_BoxData value2 = new Tower_BoxData
					{
						QuizType = 0,
						BoxGrade = 1,
						QuestionNum = 0,
						UNK2 = false,
						Question = string.Empty,
						Answer = string.Empty
					};
					TowerEvent_BoxList.TryAdd(i, value2);
				}
				else
				{
					if (i > 300)
					{
						continue;
					}
					if (value.Count <= 0)
					{
						Tower_BoxData value3 = new Tower_BoxData
						{
							QuizType = 0,
							BoxGrade = 1,
							QuestionNum = 0,
							UNK2 = false,
							Question = string.Empty,
							Answer = string.Empty
						};
						TowerEvent_BoxList.TryAdd(i, value3);
					}
					else
					{
						Tower_BoxData value4 = value.OrderBy((KeyValuePair<int, Tower_BoxData> _) => Guid.NewGuid()).FirstOrDefault().Value;
						TowerEvent_BoxList.TryAdd(i, value4);
					}
				}
			}
			BroadcastToAll(new TowerEvent_TimeInfo_ACK(1));
			BroadcastToAll(new TowerEvent_RemainBox2_ACK(TowerEvent_BoxList, 1));
		}

		public bool GetBox(int BoxID, int BoxType)
		{
			Tower_BoxData value2;
			if (ServerStatus.TowerEventEnable && TowerEvent_BoxList.TryGetValue(BoxID, out var value) && value.BoxGrade == BoxType)
			{
				return TowerEvent_BoxList.TryRemove(BoxID, out value2);
			}
			return false;
		}

		public bool GetBox2(int BoxID, int BoxType, string Answer)
		{
			Tower_BoxData value2;
			if (ServerStatus.TowerEventEnable && TowerEvent_BoxList.TryGetValue(BoxID, out var value) && value.BoxGrade == BoxType && value.Answer == Answer)
			{
				return TowerEvent_BoxList.TryRemove(BoxID, out value2);
			}
			return false;
		}

		public void TowerEventEnd()
		{
			if (RoomKindID != 74)
			{
				return;
			}
			TowerEvent_BoxList.Clear();
			foreach (Account item in PlayerList())
			{
				item.TowerEventJoined = false;
			}
			BroadcastToAll(new TowerEvent_TimeInfo_ACK(1));
		}

		public void BonusStageSelect(int randseed)
		{
			if (ServerSettingHolder.ServerSettings.UseBonusStageSystem && (RoomKindID == 0 || RoomKindID == 1 || RoomKindID == 4 || RoomKindID == 5) && new Random(randseed).Next(0, 100) + 1 <= ServerSettingHolder.ServerSettings.BonusStageSelectRate)
			{
				int key = 8;
				if (RoomKindID == 1 || RoomKindID == 5)
				{
					key = 30;
				}
				if (MapHolder.BonusStageMapInfos.TryGetValue(key, out var value))
				{
					BonusStageMapInfo bonusStageMapInfo = value.NextWithReplacement();
					PlayingMapNum = bonusStageMapInfo.MapNum;
					BonusStageLevel = bonusStageMapInfo.LevelType;
					BroadcastToAgent(new RM_To_AG_PlayingUpdate(ID, isPlaying: true, BonusStageLevel));
				}
			}
		}

		public void BonusStageRewardGen()
		{
			int key = 8;
			if (RoomKindID == 1 || RoomKindID == 5)
			{
				key = 30;
			}
			int num = PlayerCount();
			for (int i = 1; i <= num; i++)
			{
				int key2 = MapHolder.BonusStageRankRewardInfos[key][i];
				if (MapHolder.BonusStageRewardItemInfos.TryGetValue(key2, out var value))
				{
					BonusStageRewardInfo.Add(i, value.OrderBy((GameRewardResult _) => Guid.NewGuid()).FirstOrDefault());
				}
			}
		}

		public void StartBonusStageThread()
		{
			Task.Run(delegate
			{
				BonusStagePointRank();
			});
		}

		private async void BonusStagePointRank()
		{
			while (isPlaying && BonusStageLevel > 0)
			{
				BroadcastToAll(new BonusStage_RankList(this, 1));
				BroadcastToAll(new BonusStage_PointList(this, 1));
				await Task.Delay(3000);
			}
		}

		public void SetLapTime(int sec, byte last)
		{
			lock (syncRoot)
			{
				if (IsSetLapTime)
				{
					return;
				}
				IsSetLapTime = true;
				BroadcastToAll(new SetClearLimitTime_Ack(sec, last));
				EndLapTime = Utility.CurrentTimeMilliseconds() + sec * 1000;
				if (RuleType == 2 || RuleType_New.Contains("arithmetic") || GameMode == 37)
				{
					return;
				}
				Task.Run(async delegate
				{
					bool gameovered = false;
					while (isPlaying && PlayerList().Exists((Account p) => p.GameEndType == 0) && !gameovered)
					{
						if (Utility.CurrentTimeMilliseconds() >= EndLapTime)
						{
							foreach (Account item in PlayerList())
							{
								GameOver(item, 0, last);
							}
							gameovered = true;
						}
						await Task.Delay(100);
					}
				});
			}
		}

		public void UpdateLapTime(byte area, int remaintime, int addsec, byte last)
		{
			lock (syncRoot)
			{
				if (!EnteredArea.Contains(area))
				{
					EnteredArea.Add(area);
					EndLapTime = Utility.CurrentTimeMilliseconds() + remaintime + addsec * 1000;
					BroadcastToAll(new EnterTimeSection_Ack(area, remaintime, addsec, last));
					if (area == 100)
					{
						StartKillBossRemainTime = remaintime + addsec * 1000;
					}
				}
			}
		}

		public void DrawItem(Account User, int time, int CapsuleID, byte rank, bool bMakeAndEat, int fixitem, byte last)
		{
			lock (CapsuleLock)
			{
				if (User.GetPumpkin || User.RealItem)
				{
					return;
				}
				if (RegCapsule.TryGetValue(CapsuleID, out var value))
				{
					int num = (value.IsReal ? 1 : 0);
					if (num == 1)
					{
						User.RealItem = true;
						RealItemAppear = true;
					}
					if (value.ItemNum == 37)
					{
						PumpkinDrop = false;
						User.GetPumpkin = true;
					}
					BroadcastToAll(new GameRoom_DrawItem(User.RoomPos, time, CapsuleID, value.ItemNum, 0, bMakeAndEat, num, last));
					RegCapsule.TryRemove(CapsuleID, out var _);
				}
				else
				{
					int itemid = ((fixitem == -1) ? RandItem(rank) : fixitem);
					BroadcastToAll(new GameRoom_DrawItem(User.RoomPos, time, CapsuleID, itemid, CapsuleNum, bMakeAndEat, 0, last));
					CapsuleNum++;
				}
			}
		}

		public void DrawItem2(Account User, int time, bool bMakeAndEat, int ItemID, byte last)
		{
			lock (CapsuleLock)
			{
				BroadcastToAll(new GameRoom_DrawItem(User.RoomPos, time, CapsuleNum, ItemID, 0, bMakeAndEat, 0, last));
				CapsuleNum++;
			}
		}

		public void RegItem(Account User, int time, int itemid, byte[] bytes, byte last)
		{
			lock (CapsuleLock)
			{
				int num = 1;
				switch (itemid)
				{
				case 37:
					if (PumpkinDrop)
					{
						return;
					}
					PumpkinDrop = true;
					User.GetPumpkin = false;
					break;
				case 38:
					if (RealItemAppear)
					{
						num = 3;
					}
					User.RealItem = false;
					if (!RealItemAppear)
					{
						Task.Run(delegate
						{
							CheckRealItemExist();
						});
					}
					if (num == 3)
					{
						RealItemBuffer = bytes;
					}
					break;
				case 40:
					User.RealItem = false;
					RealItemBuffer = bytes;
					break;
				}
				for (int i = 1; i <= num; i++)
				{
					MapDrawItemInfo value = new MapDrawItemInfo
					{
						ItemNum = itemid,
						IsReal = ((itemid == 38 && i == 1) || itemid == 40)
					};
					if (RegCapsule.TryAdd(CapsuleNum, value))
					{
						BroadcastToAll(new GameRoom_RegItem(time, itemid, CapsuleNum, bytes, last));
						CapsuleNum++;
					}
				}
			}
		}

		public void RegItem2(Account User, int time, int itemid, byte[] bytes, byte last)
		{
			lock (CapsuleLock)
			{
				MapDrawItemInfo value = new MapDrawItemInfo
				{
					ItemNum = itemid,
					IsReal = (itemid == 38 || itemid == 40)
				};
				if (RegCapsule.TryAdd(CapsuleNum, value))
				{
					BroadcastToAll(new GameRoom_RegItem(time, itemid, CapsuleNum, bytes, last));
					CapsuleNum++;
				}
			}
		}

		private async void CheckRealItemExist()
		{
			RealItemLastCheckTime = Utility.CurrentTimeMilliseconds() + 20000;
			while (isPlaying)
			{
				try
				{
					bool flag = PlayerList().Exists((Account p) => p.RealItem);
					if (Utility.CurrentTimeMilliseconds() >= RealItemLastCheckTime && !flag && RealItemAppear)
					{
						lock (CapsuleLock)
						{
							for (int i = 1; i <= 3; i++)
							{
								MapDrawItemInfo value = new MapDrawItemInfo
								{
									ItemNum = 38,
									IsReal = (i == 1)
								};
								if (RegCapsule.TryAdd(CapsuleNum, value))
								{
									BroadcastToAll(new GameRoom_RegItem(GetCurrentTime(), 38, CapsuleNum, RealItemBuffer, 16));
									CapsuleNum++;
								}
							}
						}
						RealItemLastCheckTime = Utility.CurrentTimeMilliseconds() + 20000;
					}
					if (flag)
					{
						RealItemLastCheckTime = Utility.CurrentTimeMilliseconds() + 20000;
					}
					await Task.Delay(1000);
				}
				catch (Exception ex)
				{
					Log.Error("Error on checking real item exist:\r\n{0}", ex.ToString());
				}
			}
		}

		private int RandItem(int rank)
		{
			int result = 13;
			int num = 0;
			int num2 = Survival;
			if (ItemType == 2)
			{
				result = 1001;
			}
			if (rank <= num2)
			{
				if (rank == 1)
				{
					num = 0;
				}
				else if (num2 > 4)
				{
					if (rank == num2)
					{
						num = 11;
					}
					else
					{
						double num3 = 10.0 / (double)(num2 - 2);
						num = (int)((double)(rank - 2) * num3) + 1;
					}
				}
				else
				{
					if (num2 < 2)
					{
						num2 = 2;
					}
					double num4 = 10.0 / (double)(num2 - 1);
					num = (int)((double)(rank - 2) * num4) + 1;
				}
			}
			if (num >= 0 && num < 12)
			{
				if (ItemType == 1)
				{
					if (IsTeamPlay != 0)
					{
						num += 100;
					}
				}
				else if (ItemType == 2)
				{
					num = ((IsTeamPlay != 0) ? (num + 300) : (num + 200));
				}
				if (MapItemHolder.CapsuleItemMapJoints.TryGetValue(PlayingMapNum, out var value))
				{
					num = value.GroupNum;
				}
				MapItemHolder.MapCapsuleItems.TryGetValue(num, out var value2);
				result = value2.NextWithReplacement().GameItemNum;
			}
			else
			{
				Log.Error("group_num < 0 || group_num >= eRankCategory_COUNT, group_num : {0}", num);
			}
			return result;
		}

		public void RandomGameOverCheckPoint(byte channelCheckPoint, byte byGameIndex, byte last)
		{
			lock (RamdomGameOverLock)
			{
				if (recvRandGameOver)
				{
					return;
				}
				recvRandGameOver = true;
				int num = 0;
				if (PlayerCount() >= 22)
				{
					num = 3;
				}
				else if (PlayerCount() >= 15)
				{
					num = 2;
				}
				else if (PlayerCount() >= 8)
				{
					num = 1;
				}
				if (num > 0)
				{
					List<byte> collection = (from i in Enumerable.Range(1, Survival)
						select (byte)i into _
						orderby Guid.NewGuid()
						select _).Take(num).ToList();
					GameOverRank.AddRange(collection);
				}
				BroadcastToAll(new RandomGameOver(channelCheckPoint, byGameIndex, GameOverRank, last));
			}
		}

		public void RandomGameOverNotify(byte channelCheckPoint, int Session, float racelen, byte last)
		{
			lock (enterroomLock)
			{
				lock (gameoverLock)
				{
					lock (RamdomGameOverLock)
					{
						RacelengthList.Add(Session, racelen);
						if (RacelengthList.Count != Survival)
						{
							return;
						}
						IEnumerable<int> enumerable = from o in RacelengthList
							orderby o.Value descending
							select o into s
							select s.Key;
						byte b = 1;
						foreach (int item in enumerable)
						{
							bool flag = GameOverRank.Contains(b);
							if (flag && AgentServer.CurrentAccounts.TryGetValue(item, out var value))
							{
								value.SendAsync(new RandomGameOver_Die1(channelCheckPoint, last));
								value.SendAsync(new RandomGameOver_Die2(channelCheckPoint, flag, last));
							}
							b = (byte)(b + 1);
						}
						GameOverRank.Clear();
						RacelengthList.Clear();
						recvRandGameOver = false;
					}
				}
			}
		}

		public void GetSortedHeroClassGameIndices()
		{
			try
			{
				Account account = (from o in PlayerList()
					orderby o.Exp descending
					select o).FirstOrDefault();
				if (account.Level >= 71)
				{
					if (HeroClassPos != account.RoomPos)
					{
						HeroClassPos = account.RoomPos;
					}
					BroadcastToAll(new GameRoom_SortedHeroClassGameIndices(1, HeroClassPos, 1));
				}
				else if (HeroClassPos != byte.MaxValue && account.Level < 71)
				{
					HeroClassPos = byte.MaxValue;
					BroadcastToAll(new GameRoom_SortedHeroClassGameIndices(0, 0, 1));
				}
			}
			catch (Exception ex)
			{
				Log.Error("SortedHeroClassGameIndices Error:{0}", ex.ToString());
				HeroClassPos = byte.MaxValue;
			}
		}

		public void CouplePointUpdate()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (Account player in from w in PlayerList()
				where w.CoupleInfo.CoupleNum > 0
				select w)
			{
				if (PlayerList().Count((Account c) => c.CoupleInfo.CoupleNum == player.CoupleInfo.CoupleNum) == 2)
				{
					if (player.CoupleInfo.CoupleLevel < 25 && DropItem.TryGetValue(player.UserNum, out var value))
					{
						int accumulateExp = (int)((float)value.EXP * 5f / 100f);
						coupleRecordCoupleExp(player.UserNum, accumulateExp);
						DropItem[player.UserNum].GameResultDesc.SetAddCouplePoint();
					}
					int num = Calc_CouplePoint(player);
					if (dictionary.ContainsKey(player.CoupleInfo.CoupleNum))
					{
						dictionary[player.CoupleInfo.CoupleNum] += num;
					}
					else
					{
						dictionary.Add(player.CoupleInfo.CoupleNum, num);
					}
				}
			}
			string text = string.Empty;
			string text2 = string.Empty;
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				if (item.Value > 0)
				{
					text += $"{item.Key},";
					text2 += $"{item.Value},";
				}
			}
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				coupleRecordCouplePoint(text, text2);
			}
		}

		private int Calc_CouplePoint(Account User)
		{
			int result = 0;
			int num = 100;
			float num2 = 0f;
			float num3 = 0f;
			if (User.CoupleInfo.CoupleLevel >= 10)
			{
				if (User.getAttr(192, out var value))
				{
					num2 += value;
				}
				if (User.getAttr(193, out value))
				{
					num3 += value;
				}
				result = (int)Math.Round((float)num + (float)num * num2 + num3, MidpointRounding.AwayFromZero);
			}
			return result;
		}

		private void coupleRecordCouplePoint(string coupleList, string couplePointList)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleRecordCouplePoint";
				mySqlCommand.Parameters.Add("coupleList", MySqlDbType.VarString).Value = coupleList;
				mySqlCommand.Parameters.Add("couplePointList", MySqlDbType.VarString).Value = couplePointList;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleRecordCouplePoint Error:{0}", ex.Message);
			}
		}

		private void coupleRecordCoupleExp(int userNum, int accumulateExp)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleRecordCoupleExp";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
				mySqlCommand.Parameters.Add("accumulateExp", MySqlDbType.Int32).Value = accumulateExp;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleRecordCoupleExp Error:{0}", ex.Message);
			}
		}

		public void BombCountingStart(Account User, byte last)
		{
			lock (RandomBombLock)
			{
				if (RecvRandomBomb)
				{
					return;
				}
				RecvRandomBomb = true;
				BroadcastToAll(new Bomb_CountingStartAck(last));
				int num = 0;
				num = ((Survival >= 12) ? 3 : ((Survival >= 9) ? 2 : ((PlayerCount() >= 6) ? 1 : 0)));
				if (num <= 0)
				{
					return;
				}
				foreach (Account p in (from w in PlayerList()
					where w.GameEndType == 0
					select w into _
					orderby Guid.NewGuid()
					select _).Take(num).ToList())
				{
					int num2 = new Random(Guid.NewGuid().GetHashCode()).Next(17, 26) * 1000;
					p.HasBomb = true;
					p.BombEndTime = Utility.CurrentTimeMilliseconds() + num2;
					BroadcastToAll(new Bomb_RaceTransferBombAck(-1, p.RoomPos, num2, last));
					Task.Run(delegate
					{
						PlayerBombCounting(p);
					});
				}
			}
		}

		public void RaceTransferBomb(Account User, int to_user, byte last)
		{
			lock (enterroomLock)
			{
				lock (gameoverLock)
				{
					lock (RandomBombLock)
					{
						RecvRandomBomb = false;
						Account ToUser = PlayerList().FirstOrDefault((Account f) => f.RoomPos == to_user);
						if (ToUser != null && User.HasBomb && !ToUser.HasBomb && ToUser.GameEndType == 0)
						{
							int left_time = (int)(User.BombEndTime - Utility.CurrentTimeMilliseconds());
							ToUser.HasBomb = true;
							ToUser.BombEndTime = User.BombEndTime;
							Task.Run(delegate
							{
								PlayerBombCounting(ToUser);
							});
							User.HasBomb = false;
							User.BombEndTime = 0L;
							BroadcastToAll(new Bomb_RaceTransferBombAck(User.RoomPos, to_user, left_time, last));
						}
					}
				}
			}
		}

		private async void PlayerBombCounting(Account User)
		{
			while (isPlaying && User.HasBomb && User.GameEndType == 0 && !User.isDisconnected)
			{
				try
				{
					if (User.BombEndTime <= Utility.CurrentTimeMilliseconds())
					{
						RecvRandomBomb = false;
						GameOver(User, 2, 16);
						User.HasBomb = false;
						User.BombEndTime = 0L;
						int left_bomb_count = PlayerList().Count((Account c) => c.HasBomb);
						BroadcastToAll(new Bomb_RaceExpldingNotify(User.RoomPos, left_bomb_count, 16));
					}
					await Task.Delay(100);
				}
				catch (Exception ex)
				{
					Log.Error("Error on processing player bomb counting:\r\n{0}", ex.ToString());
				}
			}
		}

		public void Dispose()
		{
			if (PlayMode != 2)
			{
				_ChangeRoomMasterTimer.Dispose();
			}
			if (Players != null)
			{
				Players.Clear();
			}
			if (LevelUPPlayerList != null)
			{
				LevelUPPlayerList.Clear();
			}
			if (KickedList != null)
			{
				KickedList.Clear();
			}
			if (DropItem != null)
			{
				DropItem.Clear();
			}
			if (PosList != null)
			{
				PosList.Clear();
			}
			if (RelayPosList != null)
			{
				RelayPosList.Clear();
			}
			if (FarmRoomMapCache != null)
			{
				FarmRoomMapCache.Clear();
			}
			isPlaying = false;
			FarmCraftMapCache = null;
		}
	}
}
