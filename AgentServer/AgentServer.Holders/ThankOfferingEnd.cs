using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using Akka.Actor;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Serilog;

namespace AgentServer.Holders
{
	public class ThankOfferingEnd : ActorBase
	{
		protected override bool Receive(object message)
		{
			int ScheduleNum = Convert.ToInt32(message);
			if (ServerSettingHolder.ServerSettings.ThankOfferingSchedule_OpenClose == "Close" || ServerSettingHolder.ServerSettings.ThankOfferingSchedule_CurNum != ScheduleNum)
			{
				return true;
			}
			Log.Debug("ThankOffering End ScheduleNum:{0}", ScheduleNum);
			ServerSettingHolder.ServerSettings.ThankOfferingSchedule_OpenClose = "Close";
			if (ServerStatus.MyAgentID == 1)
			{
				ThankOfferingEvent_End(ScheduleNum);
				ServerStatus.LBServerActor.Tell(new ReloadSetting
				{
					Code = 8
				});
				ServerStatus.ToAllRoomServer(new ReloadSetting
				{
					Code = 8
				});
				KeyValuePair<int, ThankOfferingSchedule> keyValuePair = (from w in ThankOfferingSystem.ThankOfferingSchedule
					where w.Key != ScheduleNum && w.Value.EndTime > DateTime.Now
					select w into o
					orderby o.Value.StartTime
					select o).FirstOrDefault();
				if (keyValuePair.Value != null)
				{
					ThankOfferingScheduleNext message2 = new ThankOfferingScheduleNext
					{
						nextScheduleNum = keyValuePair.Key,
						nextStartTime = keyValuePair.Value.StartTime,
						nextEndTime = keyValuePair.Value.EndTime
					};
					ServerStatus.LBServerActor.Tell(message2);
				}
				ServerStatus.LBServerActor.Tell(new NoticePacket("開發者聯賽已經結束", 1));
				Log.Debug("ThankOffering End");
			}
			ThankOfferingSystem.ThankOfferingSchedule.Remove(ScheduleNum);
			return true;
		}

		private bool ThankOfferingEvent_End(int ScheduleNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_ThankOfferingEvent_End";
				mySqlCommand.Parameters.Add("ScheduleNum", MySqlDbType.Int32).Value = ScheduleNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_ThankOfferingEvent_End Error: {0}", ex.Message);
			}
			return false;
		}
	}
}
