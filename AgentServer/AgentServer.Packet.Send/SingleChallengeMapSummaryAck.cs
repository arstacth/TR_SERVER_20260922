using System;
using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Packed CHALLENGE_MAP_SUMMARY_ACK (wire 1628).
	/// 35-byte all-zero body: RemainSize=29 → parser consumed opcode+int32(0) and
	/// stopped. 203-byte result=1 body: RemainSize=169 → opcode+int32+28.
	/// First int is count; each record is popRawData(28).
	/// </summary>
	public sealed class SingleChallengeMapSummaryAck : NetPacket
	{
		public SingleChallengeMapSummaryAck(int userNum, IList<int> mapNums, byte last)
		{
			Dictionary<int, int[]> records = LoadUserRecords(userNum);
			ns.WriteOP(Opcodes.eServer_CHALLENGE_MAP_SUMMARY_ACK);
			int count = mapNums != null ? mapNums.Count : 0;
			ns.Write(count);
			for (int i = 0; i < count; i++)
			{
				int map = mapNums[i];
				int medal = 0;
				int goal = 0;
				if (records.TryGetValue(map, out int[] rec))
				{
					medal = rec[0];
					goal = rec[1];
				}
				ns.Write(map);
				ns.Write(medal);
				ns.Write(goal);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0L);
			}
			_ = last;
		}

		private static Dictionary<int, int[]> LoadUserRecords(int userNum)
		{
			Dictionary<int, int[]> records = new Dictionary<int, int[]>();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(
				"SELECT fdMapNum, fdMedalType, fdGoalSec FROM usersinglechallengeinfo WHERE fdUserNum = @u",
				mySqlConnection);
			mySqlCommand.Parameters.Add("u", MySqlDbType.Int32).Value = userNum;
			using MySqlDataReader reader = mySqlCommand.ExecuteReader();
			while (reader.Read())
			{
				records[Convert.ToInt32(reader["fdMapNum"])] = new int[]
				{
					Convert.ToInt32(reader["fdMedalType"]),
					Convert.ToInt32(reader["fdGoalSec"])
				};
			}
			return records;
		}
	}
}
