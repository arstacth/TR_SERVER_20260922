using System;
using System.Data;
using MySql.Data.MySqlClient;
using RoomServer.Structuring;

namespace RoomServer.Packet
{
	public class LobbyHandle
	{
		public static bool LevelUPCheck_RM(Account User, int beforelevel)
		{
			User.GetMyLevel();
			return User.Level != beforelevel;
		}

		private static void requestUserPoint(int UserNum, int rewardGroup, out int totlapoint, out int currentpoint)
		{
			totlapoint = 0;
			currentpoint = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_requestUserPoint";
			mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
			mySqlCommand.Parameters.Add("rewardGroup", MySqlDbType.Int32).Value = rewardGroup;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			totlapoint = Convert.ToInt32(mySqlDataReader["pointAccumulated"]);
			currentpoint = Convert.ToInt32(mySqlDataReader["pointCurrent"]);
		}
	}
}
