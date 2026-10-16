using System;
using System.Data;
using CommunityAgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace CommunityAgentServer.Packet.Send
{
	public sealed class GetProfileByNickName : NetPacket
	{
		public GetProfileByNickName(string NickName, short unk1)
			: base(3, 0)
		{
			byte value = 0;
			byte value2 = 0;
			string value3 = string.Empty;
			string value4 = string.Empty;
			string value5 = string.Empty;
			bool flag = false;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_profileGet";
				mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (flag = mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					value = Convert.ToByte(mySqlDataReader["Sex"]);
					value2 = Convert.ToByte(mySqlDataReader["Location"]);
					value3 = mySqlDataReader["Age"].ToString();
					value4 = mySqlDataReader["Job"].ToString();
					value5 = mySqlDataReader["Hobby"].ToString();
				}
			}
			ns.WriteOP(eCommunityAgentOpcode.PROFILE_ACK);
			ns.WriteOP(eCommunityAgentProfile.GET_BY_NICK_REQ);
			ns.Write((byte)0);
			ns.Write(unk1);
			ns.Write(0L);
			if (flag)
			{
				ns.Write(2);
				ns.Write((byte)1);
				ns.Write(value);
				ns.Write((byte)3);
				ns.Write(value2);
				ns.Write(3);
				ns.Write((byte)2);
				ns.WriteBIG5Fixed_shortSize(value3);
				ns.Write((byte)4);
				ns.WriteBIG5Fixed_shortSize(value4);
				ns.Write((byte)5);
				ns.WriteBIG5Fixed_shortSize(value5);
			}
			else
			{
				ns.Write(0L);
			}
		}
	}
}
