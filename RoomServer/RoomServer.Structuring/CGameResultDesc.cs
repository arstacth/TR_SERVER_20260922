namespace RoomServer.Structuring
{
	public class CGameResultDesc
	{
		public enum eBonusDesc
		{
			eBonusDesc_BEGINNER_EXP_BONUS = 1,
			eBonusDesc_PCROOM_ADD_BONUS = 2,
			eBonusDesc_ITEM_TRUP_BONUS = 4,
			eBonusDesc_ITEM_EXPUP_BONUS = 8,
			eBonusDesc_TR_COIN_BONUS = 16,
			eBonusDesc_LEVEL_UP = 32,
			eBonusDesc_TR_2X_MAP_BONUS = 64,
			eBonusDesc_EXP_2X_MAP_BONUS = 128,
			eBonusDesc_TR_2X_BONUS = 256,
			eBonusDesc_EXP_2X_BONUS = 512,
			eBonusDesc_LEVEL_UP_REWARD_GAMEMONEY_2X = 1024,
			eBonusDesc_TR_2X_CHANNEL_BONUS = 2048,
			eBonusDesc_EXP_2X_CHANNEL_BONUS = 4096,
			eBonusDesc_HANGAME_SPCROOM_BONUS = 8192,
			eBonusDesc_TREASURE_ITEM_ADD_BONUS_EXP = 16384,
			eBonusDesc_TREASURE_ITEM_ADD_BONUS_TR = 32768,
			eBonusDesc_GUILDPOINT_ADD_BONUS = 65536,
			eBonusDesc_COUPLEPOINT_ADD_BONUS = 131072,
			eBonusDesc_FREE_PCROOM_BONUS = 262144,
			eBonusDesc_PREMIUM_PCROOM_BONUS = 524288,
			eBonusDesc_ASSIST_ITEM_ADD_BONUS_EXP = 1048576,
			eBonusDesc_FREE_PASS_BUFF_BONUS = 2097152,
			eBonusDesc_MAX = 2097153
		}

		private uint m_data;

		public void clear()
		{
			m_data = 0u;
		}

		public void SetBeginnerExpBonus()
		{
			m_data |= 1u;
		}

		public void SetPCRoomAddBonus()
		{
			m_data |= 2u;
		}

		public void SetItemTRBonus()
		{
			m_data |= 4u;
		}

		public void SetItemEXPBonus()
		{
			m_data |= 8u;
		}

		public void SetTRCoinBonus()
		{
			m_data |= 16u;
		}

		public void SetLevelUP()
		{
			m_data |= 32u;
		}

		public void SetTR2XMapBonus()
		{
			m_data |= 64u;
		}

		public void SetExp2XMapBonus()
		{
			m_data |= 128u;
		}

		public void SetTR2XBonus()
		{
			m_data |= 256u;
		}

		public void SetExp2XBonus()
		{
			m_data |= 512u;
		}

		public void SetLevelUPRewardGameMoney2X()
		{
			m_data |= 1024u;
		}

		public void SetTR2XChannelBonus()
		{
			m_data |= 2048u;
		}

		public void SetEXP2XChannelBonus()
		{
			m_data |= 4096u;
		}

		public void SetHangameSPCRoomBonus()
		{
			m_data |= 8192u;
		}

		public void SetTreasureItemAddBonusExp()
		{
			m_data |= 16384u;
		}

		public void SetTreasureItemAddBonusTR()
		{
			m_data |= 32768u;
		}

		public void SetItemAddGuildPoint()
		{
			m_data |= 65536u;
		}

		public void SetAddCouplePoint()
		{
			m_data |= 131072u;
		}

		public void SetFreePCRoomBonus()
		{
			m_data |= 262144u;
		}

		public void SetPremiumPCRoomBonus()
		{
			m_data |= 524288u;
		}

		public void SetAssistItemAddBonusExp()
		{
			m_data |= 1048576u;
		}

		public void SetFreePassBuffBonus()
		{
			m_data |= 2097152u;
		}

		public void SetRawData(uint data)
		{
			m_data = data;
		}

		public uint GetRawData()
		{
			return m_data;
		}
	}
}
