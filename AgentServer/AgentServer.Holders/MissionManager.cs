using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentServer.Network.Connections;
using AgentServer.Packet;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using Akka.Actor;
using Serilog;

namespace AgentServer.Holders
{
	public class MissionManager : ActorBase
	{
		protected override bool Receive(object message)
		{
			switch (Convert.ToByte(message))
			{
			case 1:
				MissionHolder.Mission_LoadToDayMissionInfo(init: false);
				if (MissionHolder.DailyMissionInfo.Count == 0)
				{
					MissionHolder.MissionReloading = true;
				}
				break;
			case 2:
				if (!MissionHolder.MissionReloading)
				{
					return true;
				}
				ServerStatus.ServerActor.Tell(new DailyMissionUserMissionDeleteNotify_ACK());
				if (ServerStatus.MyAgentID == 1)
				{
					MissionHolder.Mission_oneDayMission_Init();
				}
				break;
			case 3:
				if (!MissionHolder.MissionReloading)
				{
					return true;
				}
				MissionHolder.Mission_LoadToDayMissionInfo(init: false);
				MissionHolder.EndReloadTime = DateTime.Now;
				MissionHolder.MissionReloading = false;
				MissionHolder.UserMissionReloading = true;
				Task.Run(delegate
				{
					UserMissionReloadThread();
				});
				foreach (Account item in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin && w.LoginDateTime < MissionHolder.EndReloadTime))
				{
					try
					{
						item.DailyMission.MissionInfo.Clear();
						Mission.mission_AddUserOneDayMissionList(item, 3, Mission.GetLevelGroup(item.Level));
						foreach (KeyValuePair<int, MissionInfo> item2 in item.DailyMission.MissionInfo)
						{
							if (!MissionHolder.ConditionGroupInfo.TryGetValue(item2.Key, out var value) || !MissionHolder.ConditionInfo.TryGetValue(value, out var value2))
							{
								continue;
							}
							foreach (MissionConditionData item3 in value2.Where((MissionConditionData w) => w.accumulative))
							{
								MissionConditionInfo value3 = new MissionConditionInfo
								{
									missionNum = item2.Key,
									conditionNum = item3.conditionNum,
									achievedPoint = 0
								};
								item.DailyMission.MissionInfo[item2.Key].ConditionInfo.Add(item3.conditionNum, value3);
							}
						}
						item.DailyMission.FinishedInfo.MissionFinishedInfo[3] = 0;
					}
					catch (Exception ex)
					{
						Log.Error("2 Init UserDailyMission Error:{0}", ex.Message);
					}
				}
				MissionHolder.UserMissionReloading = false;
				break;
			}
			return true;
		}

		private async void UserMissionReloadThread()
		{
			while (MissionHolder.UserMissionReloading)
			{
				await Task.Delay(1000);
			}
			await Task.Delay(500);
			ServerStatus.ServerActor.Tell(new ReloadDailyMission());
		}
	}
}
