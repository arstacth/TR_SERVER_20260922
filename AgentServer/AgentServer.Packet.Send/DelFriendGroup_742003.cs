using System;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class DelFriendGroup_742003 : NetPacket
	{
		public DelFriendGroup_742003(Account User, short groupnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_FRIEND_GROUP_ACK);
			ns.Write((byte)3);
			int num = (int)ns.Position;
			int num2 = 0;
			ns.Write(num2);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_cm_groupOperate";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("operationType", MySqlDbType.Int16).Value = 3;
				mySqlCommand.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupnum;
				mySqlCommand.Parameters.Add("isFolding", MySqlDbType.Byte).Value = 0;
				mySqlCommand.Parameters.Add("groupName", MySqlDbType.VarString).Value = "";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						ns.Write(Convert.ToInt16(mySqlDataReader["fdGroupNum"]));
						ns.Write(0);
						ns.Write(Convert.ToBoolean(mySqlDataReader["fdIsFolding"]));
						ns.Write((short)0);
						num2++;
					}
				}
			}
			_ = last;
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(num2);
			ns.Seek(0L, SeekOrigin.End);
		}
	}
}
