using System;
using System.Data;
using System.IO;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class SingleChallengeUserInfo : NetPacket
	{
		public SingleChallengeUserInfo(string NickName, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_INFO_ACK__CHALLENGE_FULLRECORD);
			// Packed 339: pop int32, nick (int16+bytes), count, then count*{map,medal,goal}.
			// debug_trgame 0x140712510: first int==0 logs
			// "CHALLENGE_FULLRECORD failed" and passes r9b=1 into vtable+0x2600.
			// Nonzero (1) skips that fail path. RemainSize=1 on the 33-byte ACK is the trailer.
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(NickName);
			int num = 0;
			int num2 = (int)ns.Position;
			ns.Write(num);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_SingleChallenge_GetUserInfo";
				mySqlCommand.Parameters.Add("NickName", MySqlDbType.VarString).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ns.Write(Convert.ToInt32(mySqlDataReader["fdMapNum"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdMedalType"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdGoalSec"]));
					num++;
				}
			}
			_ = last;
			ns.Seek(num2, SeekOrigin.Begin);
			ns.Write(num);
		}
	}
}
