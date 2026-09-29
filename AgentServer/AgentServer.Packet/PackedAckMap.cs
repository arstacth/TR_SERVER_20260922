using System;
using System.Collections.Generic;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using Serilog;

namespace AgentServer.Packet
{
	/// <summary>
		/// Lobby REQs with no dedicated handler (empty ACK stubs).
		/// Only _REQ→_ACK pairs that exist in tools/trgame_NATIVE_OPCODE_TABLE.csv.
		/// Stub body is N ints + last; dbgtrace Remain/Overpop is the layout source.
	/// </summary>
	public static class PackedAckMap
	{
		private static readonly Dictionary<ushort, ushort> ReqWireToAckWire = new Dictionary<ushort, ushort>
		{
			{ (ushort)Opcodes.eServer_PACKED_771_REQ, (ushort)Opcodes.eServer_MESSAGE_GET_FRIEND_ADD_COUNT_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1740_REQ, (ushort)Opcodes.eServer_PACKED_1717_ACK },
			{ (ushort)Opcodes.eServer_FASHION_COORDI_KING_TOP3RANK_REQ, (ushort)Opcodes.eServer_PACKED_1641_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1660_REQ, (ushort)Opcodes.eServer_PACKED_939_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2343_REQ, (ushort)Opcodes.eServer_PACKED_2344_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2345_REQ, (ushort)Opcodes.eServer_PACKED_2346_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1405_REQ, (ushort)Opcodes.eServer_PACKED_1398_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2035_REQ, (ushort)Opcodes.eServer_PACKED_1922_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1630_REQ, (ushort)Opcodes.eServer_PACKED_1720_ACK },
			{ (ushort)Opcodes.eServer_PACKED_914_REQ, (ushort)Opcodes.eServer_PACKED_1907_ACK },
			{ (ushort)Opcodes.eServer_PACKED_353_REQ, (ushort)Opcodes.eServer_PACKED_145_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2436_REQ, (ushort)Opcodes.eServer_PACKED_2437_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2440_REQ, (ushort)Opcodes.eServer_PACKED_2441_ACK },
			{ (ushort)Opcodes.eServer_PACKED_661_REQ, (ushort)Opcodes.eServer_PACKED_1989_ACK },
			// StatSystem save/set (native table wires).
			{ 525, 1101 },
			{ 565, 1610 },
			{ 1041, 583 },
			{ (ushort)Opcodes.eServer_PACKED_976_REQ, (ushort)Opcodes.eServer_PACKED_669_ACK },
			{ (ushort)Opcodes.eServer_PACKED_206_REQ, (ushort)Opcodes.eServer_PACKED_943_ACK },
			{ (ushort)Opcodes.eServer_PACKED_302_REQ, (ushort)Opcodes.eServer_PACKED_135_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1066_REQ, (ushort)Opcodes.eServer_PACKED_401_ACK },
			{ (ushort)Opcodes.eServer_PACKED_1623_REQ, (ushort)Opcodes.eServer_PACKED_1481_ACK },
			{ (ushort)Opcodes.eServer_PACKED_885_REQ, (ushort)Opcodes.eServer_PACKED_1562_ACK },
			{ (ushort)Opcodes.eServer_PACKED_2357_REQ, (ushort)Opcodes.eServer_PACKED_2358_ACK },
		};

		// Default stub is two ints (size 11 Remain 1). 19:23 dbgtrace:
		// Overpop 10 11 4 = need a third int; Remain 5 size 11 = one int only.
		private static readonly Dictionary<ushort, int> AckWireIntCount = new Dictionary<ushort, int>
		{
			{ (ushort)Opcodes.eServer_PACKED_1641_ACK, 3 },
			{ (ushort)Opcodes.eServer_PACKED_1398_ACK, 3 },
			{ (ushort)Opcodes.eServer_PACKED_145_ACK, 3 },
			{ (ushort)Opcodes.eServer_PACKED_2441_ACK, 5 },
			{ (ushort)Opcodes.eServer_PACKED_1989_ACK, 4 },
			{ (ushort)Opcodes.eServer_PACKED_2437_ACK, 1 },
			{ (ushort)Opcodes.eServer_PACKED_669_ACK, 1 },
			{ (ushort)Opcodes.eServer_PACKED_943_ACK, 2 },
			{ (ushort)Opcodes.eServer_PACKED_135_ACK, 1 },
			{ (ushort)Opcodes.eServer_PACKED_401_ACK, 1 },
			// ENTRY_SYSTEM_SHOP_SCHEDULE_NOTIFY: 2 ints Overpop; 4 ints RemainSize=4 → 3 ints no last.
			// Overpop 7 14 8 — client pops byte+int then long; need OP(2)+byte(1)+int(4)+long(8)=15 min.
			{ (ushort)Opcodes.eServer_PACKED_1481_ACK, 3 },
			// CUSTOM_PACKAGE_DATA_ACK: 2 ints RemainSize=4 on size 10 → one int.
			{ (ushort)Opcodes.eServer_PACKED_2358_ACK, 1 },
		};

		public static bool TrySendEmptyAck(ClientConnection client, ushort wire, PacketReader reader, byte last)
		{
			if (wire == (ushort)Opcodes.eServer_PACKED_661_REQ)
			{
				var acc = client.CurrentAccount;
				client.SendAsync(new StatSystemMyInfoAck(last, acc != null ? acc.StatSystemPageCount : 1, acc?.StatSystemTitles));
				return true;
			}
			// StatSystem SET_PAGENUM — third int is payPageResult (0=OK), NOT Hold SP.
			if (wire == 565)
			{
				int page = 0;
				if (reader.Remaining >= 4)
				{
					page = reader.ReadLEInt32();
				}
				if (page < 0 || page >= 10)
				{
					page = 0;
				}
				int maxPage = client.CurrentAccount != null ? client.CurrentAccount.StatSystemPageCount : 1;
				if (maxPage < 1)
				{
					maxPage = 1;
				}
				if (page >= maxPage)
				{
					page = maxPage - 1;
				}
				if (client.CurrentAccount != null)
				{
					try
					{
						using MySqlCommandHelper cmd = new MySqlCommandHelper("usp_StatSystem_SetPageNum");
						cmd.AddParamInt("usernum", client.CurrentAccount.UserNum);
						cmd.AddParamInt("pageNum", page);
						cmd.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						Log.Warning("usp_StatSystem_SetPageNum: {0}", ex.Message);
					}
				}
				client.SendAsync(new StatSystemSetPageNumAck(page, last));
				return true;
			}
			// StatSystem SAVE_PAGE — accept stub save; echo page + remain SP.
			if (wire == 525)
			{
				int page = 0;
				if (reader.Remaining >= 4)
				{
					page = reader.ReadLEInt32();
				}
				client.SendAsync(new StatSystemSavePageAck(page, StatSystemSp.Remain(), last));
				return true;
			}
			// StatSystem SAVE_TITLE — page is 0-based; ignore out-of-range (broken UI sent lastSlot=31).
			if (wire == 1041)
			{
				int page = 0;
				string title = string.Empty;
				if (reader.Remaining >= 4)
				{
					page = reader.ReadLEInt32();
				}
				if (reader.Remaining >= 2)
				{
					short titleLen = reader.ReadLEInt16();
					if (titleLen > 0 && reader.Remaining >= titleLen)
					{
						title = reader.ReadBig5StringSafe(titleLen) ?? string.Empty;
					}
				}
				if (client.CurrentAccount != null)
				{
					int idx = page;
					// Broken MyInfo used to echo SP as lastSlot; client then SaveTitle(page=999).
					if (idx < 0 || idx >= 10)
					{
						idx = 0;
					}
					int maxPage = client.CurrentAccount.StatSystemPageCount;
					if (maxPage < 1)
					{
						maxPage = 1;
					}
					if (idx < maxPage)
					{
						client.CurrentAccount.StatSystemTitles[idx] = title;
					}
					else
					{
						client.CurrentAccount.StatSystemTitles[idx] = title;
						client.CurrentAccount.StatSystemPageCount = idx + 1;
					}
					page = idx;
					try
					{
						using MySqlCommandHelper cmd = new MySqlCommandHelper("usp_StatSystem_SetPageTitle");
						cmd.AddParamInt("usernum", client.CurrentAccount.UserNum);
						cmd.AddParamInt("pageNum", page);
						cmd.AddParamVarString("title", title ?? string.Empty);
						cmd.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						Log.Warning("usp_StatSystem_SetPageTitle: {0}", ex.Message);
					}
				}
				client.SendAsync(new StatSystemSaveTitleAck(page, title, last));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_2381_REQ && client.CurrentAccount != null)
			{
				client.SendAsync(new CashPointAck(client.CurrentAccount, last));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_1405_REQ)
			{
				// STORAGE_USER_COUNT_REQ → real keeping/max counts (not zero stub).
				client.SendAsync(new StorageUserCountAck(client.CurrentAccount, last));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_885_REQ)
			{
				// 885 = DOM_INFO_REQ → 1562. Live empty ACK was 48 bytes; size 6 Overpops +4.
				client.SendAsync(new PackedEmptyAck((ushort)Opcodes.eServer_PACKED_1562_ACK, last, 11, writeLast: false, extraBytes: 2));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_214_REQ)
			{
				int period = 0;
				if (reader.Remaining >= 4)
				{
					period = reader.ReadLEInt32();
				}
				// Open window from Gregorian competition period (DB) or now±span.
				// Always InvariantCulture / BE→CE via GameDate — never CurrentCulture parse.
				long open;
				long close;
				if (!TryCompetitionPeriodTimestamps(out open, out close))
				{
					long now = Utility.CurrentTimeMilliseconds();
					open = now - 3600000L;
					close = now + 86400000L * 365L * 50L;
				}
				client.SendAsync(new PackedEntrySystemPeriodNotify(period, open, close, last));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_1623_REQ)
			{
				int shopNum = 0;
				if (reader.Remaining >= 4)
				{
					shopNum = reader.ReadLEInt32();
				}
				// debug_trgame 0x762840: i32 + u8 openFlag + i64. openFlag must be 1.
				client.SendAsync(new Packed1481Ack(shopNum, last));
				return true;
			}
			if (wire == (ushort)Opcodes.eServer_PACKED_206_REQ)
			{
				// REQ CE-00-<day>-<last>; ACK 943 expected OP+2 ints+last (Remain 20 on 7-int stub).
				int day = 0;
				if (reader.Remaining >= 1)
				{
					day = reader.ReadByte();
				}
				client.SendAsync(new NewAttendanceRecvRewardAck(0, day, last));
				return true;
			}
			if (!ReqWireToAckWire.TryGetValue(wire, out ushort ack))
			{
				return false;
			}
			if (ack == (ushort)Opcodes.eServer_PACKED_1481_ACK)
			{
				client.SendAsync(new Packed1481Ack(last));
				return true;
			}
			int ints = 2;
			if (AckWireIntCount.TryGetValue(ack, out int n))
			{
				ints = n;
			}
			client.SendAsync(new PackedEmptyAck(ack, last, ints));
			return true;
		}

		private static bool TryCompetitionPeriodTimestamps(out long open, out long close)
		{
			open = 0;
			close = 0;
			string startText = null;
			string endText = null;
			try
			{
				foreach (var item in ServerSettingHolder.ServerSettingList)
				{
					if (item == null || string.IsNullOrWhiteSpace(item.Value))
					{
						continue;
					}
					if (string.Equals(item.Key, "CompetitionEventPeriod_StartDateTime", StringComparison.OrdinalIgnoreCase))
					{
						startText = item.Value;
					}
					else if (string.Equals(item.Key, "CompetitionEventPeriod_EndDateTime", StringComparison.OrdinalIgnoreCase))
					{
						endText = item.Value;
					}
				}
			}
			catch
			{
				return false;
			}
			DateTime startDt;
			DateTime endDt;
			if (!GameDate.TryParse(startText ?? "2026-01-01 00:00:00", out startDt)
				|| !GameDate.TryParse(endText ?? "2099-12-31 23:59:59", out endDt))
			{
				return false;
			}
			open = GameDate.ToTimestamp(startDt);
			close = GameDate.ToTimestamp(endDt);
			return close > open;
		}
	}
}
