using System;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using Akka.Actor;
using NetMsg.Room;

namespace AgentServer.Holders
{
	public class TowerEventReady : ActorBase
	{
		protected override bool Receive(object message)
		{
			byte type = Convert.ToByte(message);
			if (ServerStatus.MyAgentID == 1)
			{
				ServerStatus.ToAllRoomServer(new TowerEventInfo
				{
					Type = type
				});
				ServerStatus.LBServerActor.Tell(new TowerEvent_Notify_ACK(type, 1));
			}
			return true;
		}
	}
}
