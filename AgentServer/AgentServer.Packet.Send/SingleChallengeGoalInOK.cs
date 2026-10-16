using System;
using System.Data;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class SingleChallengeGoalInOK : NetPacket
	{
		public SingleChallengeGoalInOK(int UserNum, int MapNum, byte MedalType, int GoalSec, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHALLENGE_MAP_END_ACK);
			// Packed 546: 10 ints. Field 7 = 0 selects goal-in (vtable+0x2610).
			// Fields 2-5 are {highestMedal, highestGoal, medal, goal}; field 6 is map.
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_SingleChallenge_GoalIn";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("MapNum", MySqlDbType.Int32).Value = MapNum;
				mySqlCommand.Parameters.Add("MedalType", MySqlDbType.Int16).Value = MedalType;
				mySqlCommand.Parameters.Add("GoalSec", MySqlDbType.Int32).Value = GoalSec;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				ns.Write(Convert.ToInt32(mySqlDataReader["type"]));
				ns.Write(Convert.ToInt32(mySqlDataReader["HighestMedalType"]));
				ns.Write(Convert.ToInt32(mySqlDataReader["HighestGoalSec"]));
				ns.Write(Convert.ToInt32(mySqlDataReader["MedalType"]));
				ns.Write(Convert.ToInt32(mySqlDataReader["GoalSec"]));
			}
			ns.Write(MapNum);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
