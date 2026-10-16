using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.Actor;
using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Guild;
using RoomServer.Structuring.Item;
using RoomServer.Structuring.Shu;
using RoomServer.Structuring.User;
using TRCommon;

namespace RoomServer.Structuring
{
	public class Account
	{
		public bool isDisconnected;

		public readonly object fishingLock = new object();

		public bool TowerEventJoined;

		public int BonusStagePoint;

		public int BonusStageExtraPoint;

		public int LadderPoint = 10000;

		public int TypingRunIndex;

		public readonly object CreateRoomLock = new object();

		private bool disposedValue;

		public int Session { get; set; }

		public int ServerID { get; set; }

		public int UserNum { get; set; }

		public IActorRef AgentServer { get; set; }

		public IActorRef AgentConnect { get; set; }

		public string UserID { get; set; } = string.Empty;


		public List<UserItemDyeing> AvatarItemDyeing { get; set; } = new List<UserItemDyeing>();


		public AdvancedAvatarInfo advancedAvatarInfo { get; set; } = new AdvancedAvatarInfo();


		public CActiveItems activeItem { get; set; } = new CActiveItems();


		public CAvatarLock avatarLock { get; set; } = new CAvatarLock();


		public CUserItemAttrManager userItemAttr { get; set; } = new CUserItemAttrManager();


		public CharAbilityAttr charAbilityAttr { get; set; } = new CharAbilityAttr();


		public short costumeMode { get; set; }

		public bool IsAvatarLock { get; set; }

		public bool AvatarLockIsCostume { get; set; }

		public string NickName { get; set; }

		public long Exp { get; set; }

		public long TR { get; set; }

		public int Cash { get; set; }

		public int Level { get; set; }

		public decimal Luck { get; set; }

		public byte[] UDPInfo { get; set; }

		public string LastIp { get; set; }

		public int Attribute { get; set; }

		public int TopRank { get; set; }

		public bool UseShu { get; set; }

		public UserShuInfo UserShuInfo { get; set; } = new UserShuInfo();


		public UserCoupleInfo CoupleInfo { get; set; } = new UserCoupleInfo();


		public int FreePassType { get; set; }

		public int AttackPoint { get; set; }

		public int HealthPoint { get; set; }

		public int DefensePoint { get; set; }

		public int PartyType { get; set; }

		public int SubPartyType { get; set; }

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

		public List<int> BonusItemMade { get; set; } = new List<int>();


		public int MyFarmUniqueNum { get; set; }

		public int CurrentFarmUniqueNum { get; set; }

		public MyFarmInfo MyFarmInfo { get; set; }

		public bool isFishing { get; set; }

		public byte FishAns1 { get; set; }

		public byte FishAns2 { get; set; }

		public CancellationTokenSource FishingCancelSource { get; set; }

		public CancellationToken FishingCancelToken { get; set; }

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

		public bool IsGuildMatching { get; set; }

		public bool RealItem { get; set; }

		public bool GetPumpkin { get; set; }

		public bool HasBomb { get; set; }

		public long BombEndTime { get; set; }

		public Dictionary<short, float> getAttrs => charAbilityAttr.m_characterAttr.m_attr.m_mapAttr;

		public bool isInRoom(out NormalRoom room)
		{
			room = Rooms.GetRoom(CurrentRoomId);
			if (InGame)
			{
				return room != null;
			}
			return false;
		}

		public void SendAsync(NetPacket packet)
		{
			AgentConnect.Tell(packet.ToArray());
		}

		public void SendAsync(byte[] msg)
		{
			AgentConnect.Tell(msg);
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
			GetPumpkin = false;
			BonusStagePoint = 0;
			BonusStageExtraPoint = 0;
			TypingRunIndex = 0;
			HasBomb = false;
			BombEndTime = 0L;
			BonusItemMade.Clear();
		}

		public void GetMyLevel()
		{
			Level = AccountHolder.LevelInfo.Count((long c) => c <= Exp) + 1;
		}

		private void UpdateUserLuck()
		{
			if (getAttr(9, out var value))
			{
				Luck = (decimal)value * 100m;
			}
		}

		private void UpdateAttackInfo()
		{
			int num = 0;
			int num2 = 100;
			int num3 = 0;
			if (getAttr(214, out var value))
			{
				num += (int)value;
			}
			if (getAttr(215, out var value2))
			{
				num3 += (int)value2;
			}
			if (getAttr(216, out var value3))
			{
				num2 += (int)value3;
			}
			AttackPoint = num;
			DefensePoint = num3;
			HealthPoint = num2;
		}

		public void charAbilityAttrMakeAttr()
		{
			charAbilityAttr.makeAttrFrom(advancedAvatarInfo, activeItem, userItemAttr, avatarLock);
			UpdateUserLuck();
			UpdateAttackInfo();
		}

		public bool getAttr(short attrType, out float value)
		{
			return charAbilityAttr.m_characterAttr.m_attr.m_mapAttr.TryGetValue(attrType, out value);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				AvatarItemDyeing = null;
				advancedAvatarInfo = null;
				avatarLock = null;
				userItemAttr.clear();
				userItemAttr = null;
				charAbilityAttr = null;
				activeItem.clear();
				activeItem = null;
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}
}
