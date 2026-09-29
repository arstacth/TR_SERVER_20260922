using System;

namespace LocalCommons.Cookie
{
	public static class Cookie
	{
		public static int Generate()
		{
			Random random = new Random();
			return random.Next(255) + (random.Next(255) << 16) + (random.Next(255) << 24);
		}
	}
}
