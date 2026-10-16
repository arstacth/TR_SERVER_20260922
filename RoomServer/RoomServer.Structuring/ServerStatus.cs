using System;
using Akka.Actor;
using RoomServer.Holders;

namespace RoomServer.Structuring
{
	public static class ServerStatus
	{
		public static bool TowerEventEnable = false;

		public static DateTime TowerEventStartTime;

		public static DateTime TowerEventEndTime;

		public static ActorSystem MainActorSystem { get; set; }

		public static IActorRef QuartzActor { get; set; }

		public static IActorRef ServerActor { get; set; }

		public static IActorRef RoomServerActor { get; set; }

		public static int MyRoomServerID { get; set; } = 1;


		public static void TowerEventInit()
		{
			TowerEventStartTime = DateTime.Now;
			TowerEventEndTime = TowerEventStartTime.AddMinutes(ServerSettingHolder.ServerSettings.TowerEventTime);
			TowerEventEnable = true;
		}

		public static void TowerEventEnd()
		{
			TowerEventEnable = false;
			TowerEventStartTime = TowerEventStartTime.AddHours(1.0);
			TowerEventEndTime = TowerEventStartTime;
		}
	}
}
