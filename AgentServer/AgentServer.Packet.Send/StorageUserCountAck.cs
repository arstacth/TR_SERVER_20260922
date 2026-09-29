using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 1398 STORAGE_USER_COUNT_ACK. Empty PackedAckMap zeros made the UI
	/// show 0 max / bogus remaining (negative). Match inspire-notify field order
	/// with real keeping/gift counts.
	/// </summary>
	public sealed class StorageUserCountAck : NetPacket
	{
		public StorageUserCountAck(Account user, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_PACKED_1398_ACK);
			int keep = 0;
			int gift = 0;
			int maxKeep = 127;
			int maxGift = 20;
			try
			{
				if (user != null)
				{
					using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
					conn.Open();
					using (MySqlCommand cmd = new MySqlCommand(
						"SELECT COUNT(*) FROM userstoragekeepingitem WHERE fdUserNum=@u", conn))
					{
						cmd.Parameters.AddWithValue("@u", user.UserNum);
						keep = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
					}
					using (MySqlCommand cmd = new MySqlCommand(
						"SELECT COUNT(*) FROM userstoragegiftitem WHERE fdUserNum=@u", conn))
					{
						cmd.Parameters.AddWithValue("@u", user.UserNum);
						gift = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
					}
					using (MySqlCommand cmd = new MySqlCommand(string.Empty, conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "usp_storage_getUserDesc";
						cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = user.UserNum;
						using MySqlDataReader rd = cmd.ExecuteReader(CommandBehavior.SingleRow);
						if (rd.Read())
						{
							maxKeep = ClampCount(rd["maxSavableCount"], 127);
							maxGift = ClampCount(rd["maxReceivableCount"], 20);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("StorageUserCountAck: {0}", ex.Message);
			}
			if (keep < 0)
			{
				keep = 0;
			}
			if (gift < 0)
			{
				gift = 0;
			}
			// result + maxSavable + keepingUsed.
			// Writing used then max made UI do used-max → negative (0-127).
			ns.Write(0);
			ns.Write(maxKeep);
			ns.Write(keep);
			_ = gift;
			_ = maxGift;
		}

		private static int ClampCount(object raw, int fallback)
		{
			try
			{
				int v = Convert.ToInt32(raw);
				if (v < 0)
				{
					// signed tinyint overflow / unset → use fallback, never wire negative
					return fallback;
				}
				if (v > 255)
				{
					return 255;
				}
				return v;
			}
			catch
			{
				return fallback;
			}
		}
	}
}
