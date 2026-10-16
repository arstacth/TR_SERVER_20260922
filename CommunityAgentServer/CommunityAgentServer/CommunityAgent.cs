using System.Net;
using Akka.Actor;
using Akka.IO;
using CommunityAgentServer.Network.Connections;

namespace CommunityAgentServer
{
	public class CommunityAgent : ReceiveActor
	{
		private readonly IActorRef _manager = UntypedActor.Context.System.Tcp();

		public CommunityAgent(EndPoint endpoint)
		{
			_manager.Tell(new Tcp.Bind(base.Self, endpoint));
			Receive(delegate(Tcp.Connected connected)
			{
				base.Sender.Tell(new Tcp.Register(UntypedActor.Context.ActorOf(Props.Create(() => new ClientConnection(Sender, connected.RemoteAddress)))));
			});
		}
	}
}
