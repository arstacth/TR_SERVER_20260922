using Akka.Actor;

namespace AgentServer.Holders
{
	public class CombinationShopManager : ActorBase
	{
		protected override bool Receive(object message)
		{
			CombinationShopHolder.scheduleCheck();
			return true;
		}
	}
}
