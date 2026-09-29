using System;

namespace Weighted_Randomizer
{
	public class ThreadAwareRandom
	{
		private static readonly Random _global = new Random();

		private Random _local;

		public ThreadAwareRandom()
		{
			int seed;
			lock (_global)
			{
				seed = _global.Next();
			}
			_local = new Random(seed);
		}

		public ThreadAwareRandom(int seed)
		{
			_local = new Random(seed);
		}

		public int Next()
		{
			return _local.Next();
		}

		public int Next(int maxValue)
		{
			return _local.Next(maxValue);
		}

		public int Next(int minValue, int maxValue)
		{
			return _local.Next(minValue, maxValue);
		}

		public double NextDouble()
		{
			return _local.NextDouble();
		}

		public long NextLong()
		{
			return _local.NextLong();
		}

		public long NextLong(long max)
		{
			return _local.NextLong(max);
		}

		public long NextLong(long min, long max)
		{
			return _local.NextLong(min, max);
		}
	}
}
