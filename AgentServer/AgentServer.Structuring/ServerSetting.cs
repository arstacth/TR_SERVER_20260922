namespace AgentServer.Structuring
{
	public class ServerSetting
	{
		public int charStatReset;

		public int charStatReset14;

		public int charStatSelectedReset;

		public int charStatUpgrade;

		public int StatSystemStatPoint = 999;

		public int StatSystemMainStatMaxPoint = 999;

		public float MultiplyTR;

		public float MultiplyEXP;

		public byte SurvivalMaxUserNum;

		public byte SurvivalMinUserNum;

		public string GateNoticeURL;

		public string QuitConfirmDialogURL;

		public string cashFillUpURL;

		public string EveryDayEventURL;

		public int LoadingTimeOutMilliSeconds;

		public float RABBIT_TURTLE_FATIGUE_DEC;

		public float RABBIT_TURTLE_FATIGUE_INC;

		public float RABBIT_TURTLE_ITEM_FATIGUE_DEC;

		public float RABBIT_TURTLE_ITEM_FATIGUE_INC;

		public byte corunModeMinPlayerNum;

		public int corunModeDecreaseEnergyRatio;

		public bool useMapNumByRoomKindIDCheck;

		public byte dungeonRaidMaxUserNum;

		public byte dungeonRaidMinUserNum;

		public float dungeonRaidGotPointConst;

		public int dungeonRaidBalanceConst;

		public byte competitionMaxUserNum;

		public byte competitionMinUserNum;

		public string countrycode;

		public int competitionEventPersonalGoalPoint;

		public int GiftItemLimitLevel;

		public byte GMRoomMaxUserNum;

		public bool competitionEventOn;

		public int useCompetitionEvent;

		public byte competitionEventMaxUserNum = 8;

		public byte competitionEventMinUserNum = 2;

		public int GuildMatchPartyMemberCount;

		public string GuildMarkRegisterURL;

		public int TowerEventTime;

		public bool useThankOfferingSystem;

		public int ThankOfferingSchedule_CurNum = 1;

		public string ThankOfferingSchedule_OpenClose = "Close";

		public string ThankOfferingSchedule_Reward = "Close";

		public bool useEasyAntiCheat;

		public bool useXignCode;

		public string competitionEventUsingRoomKind;
	}
}
