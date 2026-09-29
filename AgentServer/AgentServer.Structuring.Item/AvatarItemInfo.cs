namespace AgentServer.Structuring.Item
{
	public class AvatarItemInfo
	{
		public short character;

		public ushort position;

		public ushort kind;

		public int itemdescnum;

		public long expireTime;

		public long gotDateTime;

		public int count;

		public int exp;

		public bool flag;

		public bool use;

		public void UpdateItemInfo(int newcount, long newexpireTime, long newgotDateTime)
		{
			count = newcount;
			expireTime = newexpireTime;
			gotDateTime = newgotDateTime;
		}

		public void Init()
		{
			flag = expireTime > 0;
		}
	}
}
