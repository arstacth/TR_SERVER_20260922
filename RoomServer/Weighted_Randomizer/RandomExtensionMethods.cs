using System;

namespace Weighted_Randomizer
{
	public static class RandomExtensionMethods
	{
		public static long NextLong(this Random random, long min, long max)
		{
			if (max < min)
			{
				throw new ArgumentOutOfRangeException("max", "max must be >= min!");
			}
			ulong num = (ulong)(max - min);
			ulong num2;
			do
			{
				byte[] array = new byte[8];
				random.NextBytes(array);
				num2 = (ulong)BitConverter.ToInt64(array, 0);
			}
			while (num2 > (ulong)(-1L - (long)((ulong.MaxValue % num + 1) % num)));
			return (long)(num2 % num) + min;
		}

		public static long NextLong(this Random random, long max)
		{
			return random.NextLong(0L, max);
		}

		public static long NextLong(this Random random)
		{
			return random.NextLong(long.MinValue, long.MaxValue);
		}
	}
}
