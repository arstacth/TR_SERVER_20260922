using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUserIndividualRecordMiscellaneous : NetPacket
	{
		public LoginUserIndividualRecordMiscellaneous(Account User, byte last)
		{
			int value = 0;
			int value2 = 0;
			int value3 = 0;
			int value4 = 0;
			int value5 = 0;
			int value6 = 0;
			int value7 = 0;
			int value8 = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_IndividualRecordGetMiscellaneous";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					value = Convert.ToInt32(mySqlDataReader["fdSClassCount"]);
					value2 = Convert.ToInt32(mySqlDataReader["fdAlchemistCount"]);
					value3 = Convert.ToInt32(mySqlDataReader["fdLicenseChallengeCount"]);
					value4 = Convert.ToInt32(mySqlDataReader["fdGrowthPotionCount"]);
					value5 = Convert.ToInt32(mySqlDataReader["fdNutritionPotionCount"]);
					value6 = Convert.ToInt32(mySqlDataReader["fdHarvestCount"]);
					value7 = Convert.ToInt32(mySqlDataReader["fdEnchantCount"]);
					value8 = Convert.ToInt32(mySqlDataReader["fdCleanCount"]);
				}
				mySqlCommand.Dispose();
				mySqlDataReader.Close();
				mySqlConnection.Close();
			}
			ns.WriteOP(Opcodes.eServer_INDIVIDUAL_MISCELLANEOUS_NOTIFY);
			ns.Write(value);
			ns.Write(value2);
			ns.Write(value3);
			ns.Write(value4);
			ns.Write(value5);
			ns.Write(value6);
			ns.Write(value7);
			ns.Write(value8);
			_ = last;
		}
	}
}
