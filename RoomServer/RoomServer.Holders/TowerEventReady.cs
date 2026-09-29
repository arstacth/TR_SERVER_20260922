using System;
using Akka.Actor;

namespace RoomServer.Holders
{
	public class TowerEventReady : ActorBase
	{
		protected override bool Receive(object message)
		{
			Convert.ToByte(message);
			return true;
		}
	}
}
