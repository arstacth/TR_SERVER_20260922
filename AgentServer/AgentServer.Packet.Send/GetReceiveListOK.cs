using System;
using System.Data;
using System.IO;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetReceiveListOK : NetPacket
	{
		public GetReceiveListOK(int messageType, int UserNum, short iPage, short iList, short iRequestCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_BOX_LIST_ACK);
			ns.Write(0);
			ns.Write(messageType);
			ns.Write(iPage);
			short value = 0;
			int num = (int)ns.Position;
			ns.Write(value);
			short num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_messageBoxReciveList";
				mySqlCommand.Parameters.Add("messageType", MySqlDbType.Int32).Value = messageType;
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("iPage", MySqlDbType.Int32).Value = iPage;
				mySqlCommand.Parameters.Add("iList", MySqlDbType.Int32).Value = iList;
				mySqlCommand.Parameters.Add("iRequestCount", MySqlDbType.Int32).Value = iRequestCount;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						ns.Write(Convert.ToInt64(mySqlDataReader["fdNum"]));
						ns.Write(Convert.ToInt16(mySqlDataReader["messageKind"]));
						ns.Write((short)0);
						ns.WriteAnsiFixed_intSize(mySqlDataReader["userName"].ToString());
						ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["sendDateTime"])));
						ns.WriteAnsiFixed_intSize(mySqlDataReader["message"].ToString());
						ns.Write(Convert.ToBoolean(mySqlDataReader["bRead"]));
						ns.Write(Convert.ToBoolean(mySqlDataReader["report"]));
						value = Convert.ToInt16(mySqlDataReader["totalCount"]);
						num2 = (short)(num2 + 1);
					}
				}
			}
			_ = last;
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(value);
			ns.Write(num2);
		}
	}
}
