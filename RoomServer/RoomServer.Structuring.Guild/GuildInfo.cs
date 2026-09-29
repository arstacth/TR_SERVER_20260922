using System.Collections.Generic;

namespace RoomServer.Structuring.Guild
{
	public class GuildInfo
	{
		public int guildNum;

		public short kind;

		public int mark;

		public string guildName = string.Empty;

		public string masterName = string.Empty;

		public long foundationDate;

		public short memberCount;

		public short memberLimit;

		public long exp;

		public long nextLevelExp;

		public long point;

		public int ladderPoint;

		public int joinLimitLevelOver;

		public int joinLimitLevelBelow;

		public int joinMethod;

		public int level;

		public string message = string.Empty;

		public int attendanceCount;

		public byte SkillPoint;

		public List<GuildSkillInfo> SkillInfos = new List<GuildSkillInfo>();
	}
}
