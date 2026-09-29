using System;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserAlarmInfo : NetPacket
	{
		public GetUserAlarmInfo(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_ALARM_LIST_ACK);
			int num = (int)ns.Position;
			short num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_CM_getUserAlarmInfo";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						ns.Write(Convert.ToInt16(mySqlDataReader["eventNum"]));
						ns.WriteAnsiFixed_intSize(mySqlDataReader["fromNickname"].ToString());
						ns.Write(Convert.ToInt32(mySqlDataReader["ext1"]));
						ns.Write(Convert.ToInt32(mySqlDataReader["ext2"]));
						ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["eventtime"])));
						ns.Write(0);
						num2 = (short)(num2 + 1);
					}
				}
			}
			_ = last;
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(num2);
		}
	}
}
