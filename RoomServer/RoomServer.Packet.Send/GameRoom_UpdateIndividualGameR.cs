using System;
using System.Data;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_UpdateIndividualGameRecord : NetPacket
	{
		public GameRoom_UpdateIndividualGameRecord(Account User, byte last)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int value = 0;
			int value2 = 0;
			int value3 = 0;
			int value4 = 0;
			int value5 = 0;
			int value6 = 0;
			if (User.Rank < 4)
			{
				num2 = ((User.Rank == 1) ? 1 : 0);
				num3 = ((User.Rank == 2) ? 1 : 0);
				num4 = ((User.Rank == 3) ? 1 : 0);
			}
			num = ((User.GameEndType == 1) ? 1 : 0);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_IndividualRecordUpdateGame";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("clear", MySqlDbType.Int32).Value = num;
				mySqlCommand.Parameters.Add("distance", MySqlDbType.Int32).Value = User.RaceDistance;
				mySqlCommand.Parameters.Add("first", MySqlDbType.Int32).Value = num2;
				mySqlCommand.Parameters.Add("second", MySqlDbType.Int32).Value = num3;
				mySqlCommand.Parameters.Add("third", MySqlDbType.Int32).Value = num4;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				mySqlDataReader.Read();
				value = Convert.ToInt32(mySqlDataReader["playCount"]);
				value2 = Convert.ToInt32(mySqlDataReader["distance"]);
				value3 = Convert.ToInt32(mySqlDataReader["clearCount"]);
				value4 = Convert.ToInt32(mySqlDataReader["firstCount"]);
				value5 = Convert.ToInt32(mySqlDataReader["secondCount"]);
				value6 = Convert.ToInt32(mySqlDataReader["thirdCount"]);
			}
			ns.WriteOP(RoomOpcodes.eRoom_INDIVIDUAL_GAME_RECORD_NOTIFY);
			ns.Write(value);
			ns.Write(value2);
			ns.Write(value3);
			ns.Write(value4);
			ns.Write(value5);
			ns.Write(value6);
			_ = last;
		}
	}
}
