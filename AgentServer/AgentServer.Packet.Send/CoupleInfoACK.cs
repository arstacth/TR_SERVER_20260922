using System;
using System.Data;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class CoupleInfoACK : NetPacket
	{
		public CoupleInfoACK(int coupleNum, byte flag, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_GET_USER_COUPLE_INFO_ACK);
			bool wroteBody = false;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleGetCoupleInfo";
				mySqlCommand.Parameters.Add("couplenum", MySqlDbType.Int32).Value = coupleNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					ns.Write(flag);
					mySqlDataReader.Read();
					ns.WriteAnsiFixed_intSize(mySqlDataReader["coupleName"].ToString());
					ns.WriteAnsiFixed_intSize(mySqlDataReader["coupleDesc"].ToString());
					ns.Write(coupleNum);
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleType"]));
					ns.Write((short)0);
					ns.Write((mySqlDataReader["createtime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["createtime"])));
					ns.Write((mySqlDataReader["marriedDatetime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["marriedDatetime"])));
					ns.Write((mySqlDataReader["ringChangedTime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ringChangedTime"])));
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleRingNum"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleRequireDays"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleLevel"]));
					ns.Write(-1);
					ns.Write(Convert.ToInt32(mySqlDataReader["accumulateExp"]));
					ns.Write(0);
					ns.Write(Convert.ToInt32(mySqlDataReader["couplePoint"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleRank"]));
					ns.Write((byte)0);
					mySqlDataReader.NextResult();
					ns.Write(2);
					while (mySqlDataReader.Read())
					{
						ns.WriteAnsiFixed_intSize(mySqlDataReader["fdNickname"].ToString());
						ns.Write(Convert.ToUInt16(mySqlDataReader["character"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["head"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["topBody"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["downBody"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["foot"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acHead"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acFace"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acHand"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acBack"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acNeck"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["pet"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["expansion"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acWrist"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acBooster"]));
						ns.Write(Convert.ToUInt16(mySqlDataReader["acTail"]));
						ns.Fill(16);
						ns.Fill(120);
					}
					wroteBody = true;
				}
			}
			if (!wroteBody)
			{
				// Mirror has-rows header with empty strings; member count 0 (wire_reads: u8 + str16 + str16 + …).
				ns.Write(flag);
				ns.WriteAnsiFixed_intSize("");
				ns.WriteAnsiFixed_intSize("");
				ns.Write(coupleNum);
				ns.Write(0);
				ns.Write((short)0);
				ns.Write(0L);
				ns.Write(0L);
				ns.Write(0L);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.Write(-1);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.Write((byte)0);
				ns.Write(0);
				// Overpop 78 78 4 then 82 82 4 — two trailing ints on empty couple.
				ns.Write(0);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
