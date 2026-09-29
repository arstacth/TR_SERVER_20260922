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
	public sealed class GetFarmSlot_Ack : NetPacket
	{
		public GetFarmSlot_Ack(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetFarmSlotListInfo_ACK);
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
				mySqlCommand.CommandText = "usp_Farm_GetFarmSlotList";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.MyFarmUniqueNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					ns.Write(Convert.ToInt32(mySqlDataReader["fdSlotNum"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdFarmTypeNum"]));
					ns.WriteAnsiFixed_intSize(mySqlDataReader["fdFarmName"].ToString());
					ns.WriteAnsiFixed_intSize(mySqlDataReader["fdFarmEtc"].ToString());
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdLatestDateTime"])));
					num2++;
				}
			}
			_ = last;
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(num2);
		}
	}
}
