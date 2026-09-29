using System.Net;
using Akka.Actor;
using Akka.IO;
using Serilog;

namespace RelayServer
{
	public class RelayServer : ReceiveActor
	{
		public static IActorRef server;

		private IPEndPoint _address;

		public RelayServer(IActorRef handler, IPEndPoint endpoint)
		{
			_address = endpoint;
			Receive<string>(delegate
			{
			});
		}
	}
}
