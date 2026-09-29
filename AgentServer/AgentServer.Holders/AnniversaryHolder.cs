using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AgentServer.Structuring.Park;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Holders
{
	public static class AnniversaryHolder
	{
		public static ConcurrentDictionary<int, AnniversaryAction> ActionInfo { get; } = new ConcurrentDictionary<int, AnniversaryAction>();


		public static ConcurrentDictionary<int, List<AnniversaryEvent>> EventInfo { get; } = new ConcurrentDictionary<int, List<AnniversaryEvent>>();


		public static void LoadAnniversaryInfo()
		{
			try
			{
				ActionInfo.Clear();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("select fdActionNum,\r\n                                                               fdActionType,\r\n                                                               fdActionValue\r\n                                                        from EssenAnniversaryAction;", mySqlConnection);
					mySqlCommand.Parameters.Clear();
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						AnniversaryAction anniversaryAction = new AnniversaryAction
						{
							ActionNum = mySqlDataReader.GetInt32("fdActionNum"),
							ActionType = mySqlDataReader.GetInt32("fdActionType"),
							ActionValue = mySqlDataReader.GetInt32("fdActionValue")
						};
						ActionInfo.TryAdd(anniversaryAction.ActionNum, anniversaryAction);
					}
				}
				EventInfo.Clear();
				using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection2.Open();
					using MySqlCommand mySqlCommand2 = new MySqlCommand("select fdAnniversaryNum,\r\n\t                                                           fdObjectNum,\r\n\t                                                           fdActionNum,\r\n\t                                                           fdConditionNum,\r\n\t                                                           fdReactionNum\r\n                                                        from EssenAnniversaryEvent;", mySqlConnection2);
					mySqlCommand2.Parameters.Clear();
					using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
					while (mySqlDataReader2.Read())
					{
						AnniversaryEvent info = new AnniversaryEvent
						{
							AnniversaryNum = mySqlDataReader2.GetInt32("fdAnniversaryNum"),
							ObjectNum = mySqlDataReader2.GetInt32("fdObjectNum"),
							ActionNum = mySqlDataReader2.GetInt32("fdActionNum"),
							ConditionNum = mySqlDataReader2.GetInt32("fdConditionNum"),
							ReactionNum = mySqlDataReader2.GetInt32("fdReactionNum")
						};
						EventInfo.AddOrUpdate(info.AnniversaryNum, new List<AnniversaryEvent> { info }, delegate(int k, List<AnniversaryEvent> v)
						{
							v.Add(info);
							return v;
						});
					}
				}
				Log.Information("Load EAnniversaryInffo Done!");
			}
			catch (Exception ex)
			{
				Log.Error("Load AnniversaryInfo Error : {0}", ex.Message);
			}
		}
	}
}
