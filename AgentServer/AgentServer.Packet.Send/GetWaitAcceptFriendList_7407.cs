using System;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetWaitAcceptFriendList_7407 : NetPacket
	{
		public GetWaitAcceptFriendList_7407(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_FRIEND_LIST_WAIT_ACK);
			int num = (int)ns.Position;
			int num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_cm_getFriendListWaitAccept";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ns.WriteAnsiFixed_intSize(mySqlDataReader["nickname"].ToString());
					ns.Write(Convert.ToInt16(mySqlDataReader["groupNum"]));
					ns.Write(0L);
					num2++;
				}
			}
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(num2);
			ns.Seek(0L, SeekOrigin.End);
		}
	}
}
