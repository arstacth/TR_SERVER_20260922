using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetRequestedToMe_741E : NetPacket
	{
		/// <summary>
		/// True when a pending inbound friend request was written.
		/// Empty ACK (two zero-length strings) makes the client fire onRecvRequestToMeFriend every login.
		/// </summary>
		public bool HasRequest { get; }

		public GetRequestedToMe_741E(Account User, byte last)
		{
			_ = last;
			HasRequest = false;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_cm_getRequestedToMe";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			if (!mySqlDataReader.HasRows || !mySqlDataReader.Read())
			{
				return;
			}
			string nick = mySqlDataReader["nickname"]?.ToString() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(nick))
			{
				return;
			}
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_REQUESTED_TO_ME_ACK);
			ns.WriteAnsiFixed_intSize(nick);
			ns.WriteAnsiFixed_intSize(mySqlDataReader["invitationmessage"]?.ToString() ?? string.Empty);
			HasRequest = true;
		}
	}
}
