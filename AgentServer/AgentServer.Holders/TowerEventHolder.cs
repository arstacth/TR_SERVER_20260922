using AgentServer.Structuring;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using Quartz;

namespace AgentServer.Holders
{
	public static class TowerEventHolder
	{
		private static IActorRef TowerEventReady;

		public static void LoadSchedule()
		{
			TowerEventReady = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new TowerEventReady()), "TowerEvent");
			ServerStatus.QuartzActor.Tell(new CreateJob(TowerEventReady, 1, TriggerBuilder.Create().WithCronSchedule("0 55 * ? * * *").Build()));
			ServerStatus.QuartzActor.Tell(new CreateJob(TowerEventReady, 2, TriggerBuilder.Create().WithCronSchedule("0 0 * ? * * *").Build()));
			string cronExpression = $"0 {ServerSettingHolder.ServerSettings.TowerEventTime} * ? * * *";
			ServerStatus.QuartzActor.Tell(new CreateJob(TowerEventReady, 3, TriggerBuilder.Create().WithCronSchedule(cronExpression).Build()));
		}
	}
}
