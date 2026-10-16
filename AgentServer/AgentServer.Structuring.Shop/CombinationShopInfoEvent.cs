namespace AgentServer.Structuring.Shop
{
	public struct CombinationShopInfoEvent
	{
		public int m_iEventNum;

		public CombinationShopSchedule m_schedule;

		public int m_SystemType;

		public byte m_iStartHour;

		public byte m_iStartMinute;

		public byte m_iEndHour;

		public byte m_iEndMinute;

		public bool m_bIsNotify;

		public string m_systemName;
	}
}
