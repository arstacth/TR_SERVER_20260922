using System;

namespace AgentServer.Function
{
	public static class Session
	{
		public static int Generate(int ServerID = 0)
		{
			Random random = new Random();
			return random.Next(255) + (ServerID << 8) + (random.Next(255) << 16) + (random.Next(255) << 24);
		}
	}
}
