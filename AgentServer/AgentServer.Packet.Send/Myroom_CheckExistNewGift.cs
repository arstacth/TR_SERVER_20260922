using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_CheckExistNewGift : NetPacket
	{
		public Myroom_CheckExistNewGift(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_NEW_GIFT_FOR_YOU_ACK);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_shopCheckExistNewGift";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (Convert.ToBoolean(mySqlDataReader["newMyroomGift"]) || Convert.ToBoolean(mySqlDataReader["newStorageGift"]))
				{
					ns.Write(1);
				}
			}
			ns.Write(0);
			_ = last;
		}
	}
}
