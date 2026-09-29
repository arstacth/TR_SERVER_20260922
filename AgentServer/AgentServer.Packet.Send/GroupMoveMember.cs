using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GroupMoveMember : NetPacket
	{
		public GroupMoveMember(Account User, short groupnum, string nickname, byte last)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_cm_groupMoveMember";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupnum;
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
				mySqlCommand.ExecuteNonQuery();
			}
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GROUP_MOVE_MEMBER_ACK);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
