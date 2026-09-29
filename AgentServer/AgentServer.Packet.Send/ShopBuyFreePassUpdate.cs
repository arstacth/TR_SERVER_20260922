using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class ShopBuyFreePassUpdate : NetPacket
	{
		public ShopBuyFreePassUpdate(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FREE_PASS_INSPIRE_NOTIFY);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_freepass_getUserDesc";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				User.FreePassType = Convert.ToInt32(mySqlDataReader["type"]);
				ns.Write(User.FreePassType);
				ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["expireTime"])));
			}
			_ = last;
		}
	}
}
