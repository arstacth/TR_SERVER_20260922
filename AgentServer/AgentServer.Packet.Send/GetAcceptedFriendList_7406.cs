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
	public sealed class GetAcceptedFriendList_7406 : NetPacket
	{
		public GetAcceptedFriendList_7406(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_FRIEND_LIST_ACCEPTED_ACK);
			int num = (int)ns.Position;
			int num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_cm_getFriendListAccepted";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ns.WriteAnsiFixed_intSize(mySqlDataReader["nickname"].ToString());
					ns.Write((short)0);
					ns.Write(Convert.ToInt32(mySqlDataReader["FarmUniqueNum"]));
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ExpireDateTime"])));
					bool flag = Convert.ToBoolean(mySqlDataReader["blocked"]);
					ns.Write(!flag);
					ns.Write(Convert.ToInt16(mySqlDataReader["groupNum"]));
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["lastLogoutTime"])));
					ns.WriteAnsiFixed_intSize(mySqlDataReader.GetString("memo"));
					ns.Write(Convert.ToInt16(mySqlDataReader["friendtype"]));
					ns.Write((short)0);
					ns.Write(flag);
					ns.Write((byte)0);
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
