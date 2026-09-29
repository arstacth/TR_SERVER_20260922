using System;
using System.Linq;
using AgentServer.Holders;
using Akka.Actor;
using LocalCommons.Network;
using NetMsg.LBS;
using NetMsg.Room;

namespace AgentServer.Structuring
{
	public static class ServerStatus
	{
		public static int MyAgentID = 0;

		public static bool RoomServerConnected = false;

		public static bool LBServerConnected = false;

		public static bool isReady = false;

		public static bool enableUseLuckyBag = true;

		public static bool CanShopOperation = false;

		public static bool ServerOpen = false;

		public static bool TowerEventEnable = false;

		public static DateTime TowerEventStartTime;

		public static DateTime TowerEventEndTime;

		public static Form1 form1;

		public static ActorSystem MainActorSystem { get; set; }

		public static IActorRef QuartzActor { get; set; }

		public static IActorRef ServerActor { get; set; }

		public static IActorRef RoomServerActor { get; set; }

		public static IActorRef LBServerActor { get; set; }

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

		public static void ToRoomServer(NetPacket np, int RoomServerID)
		{
			IActorRef value;
			if (RoomServerID == 0)
			{
				RoomServer.RoomServerList.Values.OrderBy((IActorRef _) => Guid.NewGuid()).FirstOrDefault().Tell(np.ToArray());
			}
			else if (RoomServer.RoomServerList.TryGetValue(RoomServerID, out value))
			{
				value.Tell(np.ToArray());
			}
		}

		public static void ToRoomServer(RM_Packet np, int RoomServerID)
		{
			IActorRef value;
			if (RoomServerID == 0)
			{
				RoomServer.RoomServerList.Values.OrderBy((IActorRef _) => Guid.NewGuid()).FirstOrDefault().Tell(np);
			}
			else if (RoomServer.RoomServerList.TryGetValue(RoomServerID, out value))
			{
				value.Tell(np);
			}
		}

		public static void ToAllRoomServer(NetPacket np)
		{
			foreach (IActorRef value in RoomServer.RoomServerList.Values)
			{
				value.Tell(np.ToArray());
			}
		}

		public static void ToAllRoomServer(ReloadSetting re)
		{
			foreach (IActorRef value in RoomServer.RoomServerList.Values)
			{
				value.Tell(re);
			}
		}

		public static void ToAllRoomServer(TowerEventInfo re)
		{
			foreach (IActorRef value in RoomServer.RoomServerList.Values)
			{
				value.Tell(re);
			}
		}
	}
}
