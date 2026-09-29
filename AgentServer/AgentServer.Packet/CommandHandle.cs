using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using Akka.Actor;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet
{
	public class CommandHandle
	{
		public class ShoutOK : NetPacket
		{
			public ShoutOK(int ShoutItemNum, byte last)
			{
				ns.WriteOP(Opcodes.eServer_USE_SHOUT_ITEM_ACK);
				ns.Write(0);
				ns.Write(ShoutItemNum);
				ns.Write(last);
			}
		}

		public class ShoutToAll : NetPacket
		{
			public ShoutToAll(string Nickname, byte type, int ShoutItemNum, string ShoutMsg, long EXP, byte last)
			{
				ns.WriteOP(Opcodes.eServer_SHOUT_ITEM_MSG_RECV);
				ns.Write(ShoutItemNum);
				ns.Write(type);
				ns.WriteAnsiFixed_intSize(ShoutMsg);
				ns.WriteAnsiFixed_intSize(Nickname);
				ns.Write(EXP);
				ns.Write(last);
			}
		}

		public static void Handle_UseShoutItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int shoutItemNum = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string shoutMsg = reader.ReadBig5StringSafe(fixedLength);
			if (UseShoutItem(currentAccount, shoutItemNum, shoutMsg))
			{
				Client.SendAsync(new ShoutOK(shoutItemNum, last));
				ServerStatus.LBServerActor.Tell(new ShoutToAll(currentAccount.NickName, 0, shoutItemNum, shoutMsg, currentAccount.Exp, last));
			}
		}

		private static bool UseShoutItem(Account User, int ShoutItemNum, string ShoutMsg)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_useShoutItem";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("ShoutItemNum", MySqlDbType.Int32).Value = ShoutItemNum;
				mySqlCommand.Parameters.Add("ShoutMsg", MySqlDbType.VarString).Value = ShoutMsg;
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				if (mySqlDataReader.GetInt32("retval") == 0)
				{
					return true;
				}
				mySqlCommand.Dispose();
				mySqlDataReader.Close();
				mySqlConnection.Close();
			}
			return false;
		}
	}
}
