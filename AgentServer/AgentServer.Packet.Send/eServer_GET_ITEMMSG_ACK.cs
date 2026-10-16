using System;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_ITEMMSG_ACK : NetPacket
	{
		public eServer_GET_ITEMMSG_ACK(Account User, int itemtype, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ITEMMSG_ACK);
			ns.Write(0);
			int num = (int)ns.Position;
			int num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemMsgPop";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("itemMsgType", MySqlDbType.Int32).Value = itemtype;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						ns.Write(Convert.ToInt32(mySqlDataReader["fdType"]));
						ns.Write(Convert.ToInt32(mySqlDataReader["fdSubType"]));
						ns.Write(Convert.ToInt32(mySqlDataReader["fdItemNum"]));
						ns.Fill(12);
						ns.Write(43458);
						ns.Write(0);
						num2++;
					}
				}
			}
			_ = last;
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(num2);
		}
	}
}
