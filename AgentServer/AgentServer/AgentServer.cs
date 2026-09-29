using System.Net;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using Akka.Actor;
using Akka.IO;
using LocalCommons.Network;
using NetMsg.LBS;
using Serilog;

namespace AgentServer
{
	public class AgentServer : ReceiveActor
	{
		private readonly IActorRef _manager = UntypedActor.Context.System.Tcp();

		public AgentServer(EndPoint endpoint)
		{
			_manager.Tell(new Tcp.Bind(base.Self, endpoint));
			Receive(delegate(Tcp.Connected connected)
			{
				IActorRef actorRef = UntypedActor.Context.ActorOf(Props.Create(() => new ClientConnection(Sender, connected.RemoteAddress)));
				base.Sender.Tell(new Tcp.Register(actorRef));
				actorRef.Tell(new ClientConnection.Net_OnConnection());
				if (!ServerStatus.isReady)
				{
					Log.Warning("Connection blocked by current setting ServerReady off");
					actorRef.Tell(new ServerNotReady());
				}
			});
			Receive(delegate(NetPacket np)
			{
				foreach (IActorRef child in UntypedActor.Context.GetChildren())
				{
					child.Tell(np);
				}
			});
			Receive(delegate(byte[] np)
			{
				foreach (IActorRef child2 in UntypedActor.Context.GetChildren())
				{
					child2.Tell(np);
				}
			});
			Receive(delegate(ReloadDailyMission np)
			{
				foreach (IActorRef child3 in UntypedActor.Context.GetChildren())
				{
					child3.Tell(np);
				}
			});
			Receive(delegate(ParkDivinationCouple np)
			{
				foreach (IActorRef child4 in UntypedActor.Context.GetChildren())
				{
					child4.Tell(np);
				}
			});
		}
	}
}
