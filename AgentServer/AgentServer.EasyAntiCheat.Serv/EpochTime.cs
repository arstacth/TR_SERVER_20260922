using System;

namespace AgentServer.EasyAntiCheat.Server
{
	internal struct EpochTime
	{
		private static DateTime Epoch;

		private long _millisecondsFromEpoch;

		public EpochTime(long millisecondsFromEpoch)
		{
			_millisecondsFromEpoch = millisecondsFromEpoch;
		}

		public EpochTime(DateTime dateTime)
		{
			_millisecondsFromEpoch = (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalMilliseconds;
		}

		public static explicit operator long(EpochTime epochTime)
		{
			return epochTime._millisecondsFromEpoch;
		}

		public static explicit operator EpochTime(long millisecondsFromEpoch)
		{
			return new EpochTime(millisecondsFromEpoch);
		}

		public static implicit operator DateTime(EpochTime epochTime)
		{
			return default(DateTime).Add(TimeSpan.FromMilliseconds(epochTime._millisecondsFromEpoch)).AddYears(Epoch.Year - 1);
		}

		public static implicit operator EpochTime(DateTime dateTime)
		{
			return new EpochTime(dateTime);
		}

		public long GetTimeInMillis()
		{
			return _millisecondsFromEpoch;
		}

		static EpochTime()
		{
			Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}
	}
}
