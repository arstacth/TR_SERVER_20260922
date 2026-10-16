using System;
using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// CHALLENGE_MAP_NPCMATCH_MAP_INFO_ACK (1745).
	/// debug_trgame 0x74AC00: count + rows keyed by MatchID (unique).
	/// Field sinks: +0x28 MapNum, +0x30 Category (after title), +0x68 MatchLevel.
	/// Title = u16 byteLen + Encoding.Default (WriteAnsiFixed_intSize).
	/// Wrong key=Category kept only 18 rows; MatchLevel slot 0 hid summer open.
	/// </summary>
	public sealed class ChallengeMapNPCMatchMapInfoAck : NetPacket
	{
		public ChallengeMapNPCMatchMapInfoAck(byte last)
		{
			List<Row> rows = LoadRows();
			ns.WriteOP(Opcodes.eServer_CHALLENGE_MAP_NPCMATCH_MAP_INFO_ACK);
			ns.Write(rows.Count);
			foreach (Row row in rows)
			{
				// key (+0x20)
				ns.Write(row.MatchId);
				// 7 ints: first → +0x28 MapNum
				ns.Write(row.MapNum);
				ns.Write(row.NpcId);
				ns.Write(row.RecordMsec);
				ns.Write(row.RewardGroup);
				ns.Write(row.ConsItem);
				ns.Write(row.ConsCount);
				ns.Write(row.RollbackItem);
				ns.WriteAnsiFixed_intSize(row.Title ?? string.Empty);
				// after title: first → +0x30 Category, last → +0x68 MatchLevel
				ns.Write(row.Category);
				ns.Write(row.AnniversaryId);
				ns.Write(row.MatchLevel);
			}
			_ = last;
		}

		private struct Row
		{
			public int MatchId;
			public int Category;
			public int MapNum;
			public int NpcId;
			public int RecordMsec;
			public int RewardGroup;
			public int ConsItem;
			public int ConsCount;
			public int RollbackItem;
			public string Title;
			public int MatchLevel;
			public int AnniversaryId;
		}

		private static List<Row> LoadRows()
		{
			List<Row> rows = new List<Row>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdMatchID, fdCategory, fdMapNum, fdNPCID, fdNPCRecordMSEC, "
					+ "fdRewardItemGroup, fdConsumptionItem, fdConsumptionItemCount, "
					+ "fdRollbackItem, fdTitle, fdMatchLevel, fdAnniversaryObjectID "
					+ "FROM EssenChallengeMode_NPCMatch ORDER BY fdMatchID",
					conn);
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					rows.Add(new Row
					{
						MatchId = Convert.ToInt32(reader[0]),
						Category = Convert.ToInt32(reader[1]),
						MapNum = Convert.ToInt32(reader[2]),
						NpcId = Convert.ToInt32(reader[3]),
						RecordMsec = Convert.ToInt32(reader[4]),
						RewardGroup = Convert.ToInt32(reader[5]),
						ConsItem = Convert.ToInt32(reader[6]),
						ConsCount = Convert.ToInt32(reader[7]),
						RollbackItem = Convert.ToInt32(reader[8]),
						Title = reader.IsDBNull(9) ? string.Empty : Convert.ToString(reader[9]),
						MatchLevel = Convert.ToInt32(reader[10]),
						AnniversaryId = Convert.ToInt32(reader[11])
					});
				}
			}
			catch (Exception ex)
			{
				Log.Warning("ChallengeMap NPCMatch load: {0}", ex.Message);
			}
			return rows;
		}
	}
}
