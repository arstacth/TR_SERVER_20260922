using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Structuring.Park;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using Serilog;

namespace AgentServer.Holders
{
	public static class CapsuleMachineHolder
	{
		public static ConcurrentDictionary<int, CapsuleMachineData> CapsuleMachineContainer { get; set; } = new ConcurrentDictionary<int, CapsuleMachineData>();


		public static int CurrentRotateNum { get; set; }

		public static void LoadCapsuleMachineInfo()
		{
			CapsuleMachineContainer.Clear();
			NestedDictionary<int, int, CapsuleMachineItemNew> nestedDictionary = new NestedDictionary<int, int, CapsuleMachineItemNew>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_capsuleMachineGetMachineInfo";
				mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader["fdMachineNum"]);
					int num = Convert.ToInt32(mySqlDataReader["fdItemNum"]);
					CapsuleMachineItemNew value = new CapsuleMachineItemNew
					{
						ItemNum = num,
						ItemCount = Convert.ToInt16(mySqlDataReader["fdItemCount"]),
						ItemMax = Convert.ToInt16(mySqlDataReader["fdItemMax"]),
						Level = Convert.ToByte(mySqlDataReader["fdLevel"]),
						ResetCount = Convert.ToInt16(mySqlDataReader["fdResetCount"]),
						RotateGroupNum = Convert.ToInt32(mySqlDataReader["fdRotateGroupNum"]),
						LastResetTime = DateTime.Now
					};
					nestedDictionary.Add(key, num, value);
				}
			}
			foreach (KeyValuePair<int, NestedDictionary<int, CapsuleMachineItemNew>> item in nestedDictionary)
			{
				CapsuleMachineData capsuleMachineData = new CapsuleMachineData();
				foreach (CapsuleMachineItemNew value2 in item.Value.Values)
				{
					capsuleMachineData.AddToItemList(value2);
				}
				capsuleMachineData.UpdateMachineInfo(item.Key);
				CapsuleMachineContainer.TryAdd(item.Key, capsuleMachineData);
			}
			CurrentRotateNum = 0;
			try
			{
				using MySqlConnection rotConn = new MySqlConnection(Conf.Connstr);
				rotConn.Open();
				using MySqlCommand rotCmd = new MySqlCommand(
					"SELECT fdMachineNum FROM GameDataRotateSettingCapsuleMachine LIMIT 1",
					rotConn);
				object o = rotCmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					CurrentRotateNum = Convert.ToInt32(o);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("capsule rotate setting: {0}", ex.Message);
			}
			if (CurrentRotateNum == 0 || !CapsuleMachineContainer.ContainsKey(CurrentRotateNum))
			{
				CurrentRotateNum = CapsuleMachineContainer.FirstOrDefault((KeyValuePair<int, CapsuleMachineData> f) => f.Value.isRotate).Key;
			}
			Log.Information("Load CapsuleMachineInfo Done! rotate={0}", CurrentRotateNum);
		}
	}
}
