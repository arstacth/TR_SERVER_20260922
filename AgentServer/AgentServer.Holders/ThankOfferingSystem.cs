using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Structuring;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using MySql.Data.MySqlClient;
using Quartz;
using Serilog;

namespace AgentServer.Holders
{
	public static class ThankOfferingSystem
	{
		public static IActorRef ThankOfferingStartActor;

		public static IActorRef ThankOfferingEndActor;

		public static Dictionary<int, ThankOfferingSchedule> ThankOfferingSchedule = new Dictionary<int, ThankOfferingSchedule>();

		public static void LoadSchedule()
		{
			ThankOfferingStartActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new ThankOfferingStart()), "ThankOfferingStart");
			ThankOfferingEndActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new ThankOfferingEnd()), "ThankOfferingEnd");
			ThankOfferingEvent_GetSchedule();
			KeyValuePair<int, ThankOfferingSchedule> keyValuePair = (from w in ThankOfferingSchedule
				where w.Value.EndTime > DateTime.Now
				select w into o
				orderby o.Value.StartTime
				select o).FirstOrDefault();
			if (keyValuePair.Value != null)
			{
				ServerStatus.QuartzActor.Tell(new CreateJob(ThankOfferingStartActor, "Init", TriggerBuilder.Create().StartAt(keyValuePair.Value.StartTime).WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
				{
					x.WithMisfireHandlingInstructionFireNow();
				})
					.Build()));
			}
			Log.Information("ThankOfferingSystem LoadSchedule:{0}", ThankOfferingSchedule.Count());
		}

		private static void ThankOfferingEvent_GetSchedule()
		{
			ThankOfferingSchedule.Clear();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_GetSchedule";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("scheduleNum");
					DateTime dateTime = mySqlDataReader.GetDateTime("startTime");
					int int2 = mySqlDataReader.GetInt32("duration");
					ThankOfferingSchedule value = new ThankOfferingSchedule
					{
						StartTime = dateTime,
						EndTime = dateTime.AddMinutes(int2)
					};
					ThankOfferingSchedule.Add(@int, value);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_GetSchedule Error: {0}", ex.Message);
			}
		}
	}
}
