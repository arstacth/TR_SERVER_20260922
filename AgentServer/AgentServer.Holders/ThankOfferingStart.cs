using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using Akka.Actor;
using Akka.Quartz.Actor.Commands;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Quartz;
using Serilog;

namespace AgentServer.Holders
{
	public class ThankOfferingStart : ActorBase
	{
		protected override bool Receive(object message)
		{
			if (message is string)
			{
				KeyValuePair<int, ThankOfferingSchedule> keyValuePair = (from w in ThankOfferingSystem.ThankOfferingSchedule
					where w.Value.EndTime > DateTime.Now
					select w into o
					orderby o.Value.StartTime
					select o).FirstOrDefault();
				if (ServerStatus.MyAgentID == 1)
				{
					Log.Debug("ThankOffering init ScheduleNum");
					if (ThankOfferingEvent_Start(keyValuePair.Key, out var endTime))
					{
						ServerStatus.LBServerActor.Tell(new ReloadSetting
						{
							Code = 8
						});
						ServerStatus.ToAllRoomServer(new ReloadSetting
						{
							Code = 8
						});
						ThankOfferingScheduleReload message2 = new ThankOfferingScheduleReload
						{
							ScheduleNum = keyValuePair.Key,
							StartTime = ThankOfferingSystem.ThankOfferingSchedule[keyValuePair.Key].StartTime,
							EndTime = endTime
						};
						ServerStatus.LBServerActor.Tell(message2);
						Log.Debug("ThankOffering init ScheduleNum");
					}
				}
				else
				{
					ServerSettingHolder.LoadServerSettingInfo();
					ServerSettingHolder.ServerSettings.ThankOfferingSchedule_OpenClose = "Open";
					ServerSettingHolder.ServerSettings.ThankOfferingSchedule_CurNum = keyValuePair.Key;
					ServerStatus.QuartzActor.Tell(new CreateJob(ThankOfferingSystem.ThankOfferingEndActor, keyValuePair.Key, TriggerBuilder.Create().StartAt(keyValuePair.Value.EndTime).WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
					{
						x.WithMisfireHandlingInstructionFireNow();
					})
						.Build()));
				}
			}
			else if (message is int)
			{
				int num = Convert.ToInt32(message);
				Log.Debug("ThankOffering Start ScheduleNum1:{0}", num);
				if (ServerStatus.MyAgentID == 1)
				{
					Log.Debug("ThankOffering Start ScheduleNum2:{0}", num);
					if (ThankOfferingEvent_Start(num, out var endTime2))
					{
						ServerStatus.LBServerActor.Tell(new ReloadSetting
						{
							Code = 8
						});
						ServerStatus.ToAllRoomServer(new ReloadSetting
						{
							Code = 8
						});
						ThankOfferingScheduleReload message3 = new ThankOfferingScheduleReload
						{
							ScheduleNum = num,
							StartTime = ThankOfferingSystem.ThankOfferingSchedule[num].StartTime,
							EndTime = endTime2
						};
						ServerStatus.LBServerActor.Tell(message3);
						ServerStatus.LBServerActor.Tell(new NoticePacket("開發者聯賽已經開始", 1));
						Log.Debug("ThankOffering Start ScheduleNum3:{0}", num);
					}
				}
			}
			return true;
		}

		private bool ThankOfferingEvent_Start(int ScheduleNum, out DateTime endTime)
		{
			endTime = DateTime.Now;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_Start";
				mySqlCommand.Parameters.Add("ScheduleNum", MySqlDbType.Int32).Value = ScheduleNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					mySqlDataReader.GetDateTime("startTime");
					endTime = mySqlDataReader.GetDateTime("endTime");
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_Start Error: {0}", ex.Message);
			}
			return false;
		}
	}
}
