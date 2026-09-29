using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUserIndividualRecordGameRecord : NetPacket
	{
		public LoginUserIndividualRecordGameRecord(Account User, byte last)
		{
			int value = 0;
			int value2 = 0;
			int value3 = 0;
			int value4 = 0;
			int value5 = 0;
			int value6 = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_IndividualRecordGetGame";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					value = Convert.ToInt32(mySqlDataReader["fdPlayCount"]);
					value2 = Convert.ToInt32(mySqlDataReader["fdClearCount"]);
					value3 = Convert.ToInt32(mySqlDataReader["fdDistance"]);
					value4 = Convert.ToInt32(mySqlDataReader["fdFirst"]);
					value5 = Convert.ToInt32(mySqlDataReader["fdSecond"]);
					value6 = Convert.ToInt32(mySqlDataReader["fdThird"]);
				}
				mySqlCommand.Dispose();
				mySqlDataReader.Close();
				mySqlConnection.Close();
			}
			ns.WriteOP(RoomOpcodes.eRoom_INDIVIDUAL_GAME_RECORD_NOTIFY);
			ns.Write(value);
			ns.Write(value3);
			ns.Write(value2);
			ns.Write(value4);
			ns.Write(value5);
			ns.Write(value6);
			_ = last;
		}
	}
}
