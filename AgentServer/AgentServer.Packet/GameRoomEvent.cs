using System.Data;
using AgentServer.Structuring;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet
{
	public class GameRoomEvent
	{
		public static void VertificationBan(Account User)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_insertBlackList";
			mySqlCommand.Parameters.Add("commandusernum", MySqlDbType.Int32).Value = 0;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = User.NickName;
			mySqlCommand.Parameters.Add("blockreason", MySqlDbType.Int32).Value = 6;
			mySqlCommand.Parameters.Add("blocktime", MySqlDbType.Int32).Value = 35;
			mySqlCommand.Parameters.Add("remoteIP", MySqlDbType.VarString).Value = User.LastIp;
			using (mySqlCommand.ExecuteReader(CommandBehavior.SingleRow))
			{
			}
		}
	}
}
