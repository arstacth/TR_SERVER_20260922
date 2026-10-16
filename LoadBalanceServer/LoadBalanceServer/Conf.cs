using System.Collections.Concurrent;
using Akka.Actor;

namespace LoadBalanceServer
{
	public static class Conf
	{
		public static string Connstr = "server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=utf8mb4";

		public static string ServerIP = "";

		public static int AgentPort = 0;

		public static int AgentPort2 = 0;

		public static int RelayPort = 0;

		public static int CommunityAgentServerPort = 0;

		public static int LoadBalanceServerPort = 0;

		public static int LBSLocalPort = 0;

		public static bool HashCheck = false;

		public static int MaxUserCount = 0;

		public static bool BlockDDOS = false;

		public static int JudgeTime = 3;

		public static int MaxConnectTime = 30;

		public static ConcurrentDictionary<IActorRef, int> AgentPortList = new ConcurrentDictionary<IActorRef, int>();

		public static ConcurrentDictionary<IActorRef, int> AgentClientBalance = new ConcurrentDictionary<IActorRef, int>();

		public static ConcurrentDictionary<IActorRef, int> AgentClientTotalLoginCount = new ConcurrentDictionary<IActorRef, int>();
	}
}
