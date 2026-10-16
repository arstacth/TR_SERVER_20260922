using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Holders
{
	public static class AccountHolder
	{
		public static Dictionary<int, byte> CharacterSexType = new Dictionary<int, byte>();

		public static HashSet<long> LevelInfo { get; } = new HashSet<long>();


		public static List<ushort> GameRoomPosition { get; } = new List<ushort>
		{
			101, 102, 105, 106, 117, 121, 122, 123, 124, 125,
			127, 135, 136, 160, 185, 186, 220, 230, 241, 242,
			247, 300, 301, 370, 800, 812
		};


		public static void LoadLevelInfo()
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essenlevelinfo WHERE fdLevelKind = 1", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					LevelInfo.Add(Convert.ToInt64(mySqlDataReader["fdExp"]));
				}
			}
			Log.Information("Load Levels Count: {0}", LevelInfo.Count());
			CharacterSexType.Clear();
			for (byte b = 1; b <= 30; b = (byte)(b + 1))
			{
				if (ItemHolder.ItemPCKDict.TryGetValue(0, b, 0, out var value))
				{
					byte value2 = 1;
					if (ItemAttrTable.getItemAttrFromItemDescNum(value, out var it))
					{
						float num = it.m_attr[79];
						if (num > 0f)
						{
							value2 = (byte)num;
						}
					}
					CharacterSexType.Add(b, value2);
				}
			}
		}
	}
}
