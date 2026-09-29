using System.Collections.Generic;
using Akka.Actor;

namespace LoadBalanceServer
{
	public static class Server
	{
		public static IActorRef AgentServerActor { get; set; }

		public static List<string> HashList { get; set; } = new List<string>();

	}
}
