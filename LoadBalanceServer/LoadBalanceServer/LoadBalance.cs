using System.Net;
using Akka.Actor;
using Akka.IO;
using LoadBalanceServer.Network.Connections;

namespace LoadBalanceServer
{
	public class LoadBalance : ReceiveActor
	{
		private readonly IActorRef _manager = UntypedActor.Context.System.Tcp();

		public LoadBalance(EndPoint endpoint)
		{
			_manager.Tell(new Tcp.Bind(base.Self, endpoint));
			Receive(delegate(Tcp.Connected connected)
			{
				base.Sender.Tell(new Tcp.Register(UntypedActor.Context.ActorOf(Props.Create(() => new ClientConnection(Sender, connected.RemoteAddress)))));
			});
		}
	}
}
