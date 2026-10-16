using System;
using System.Globalization;
using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class SEASON_CHANNEL_SCHEDULE_ACK : NetPacket
	{
		public SEASON_CHANNEL_SCHEDULE_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SEASON_CHANNEL_SCHEDULE_ACK);
			// debug_trgame 0x77CC00: count + (i32 id + str + str + str).
			// Wire dates are always Gregorian (2026…). GameDate converts BE→CE if DB/OS gave 2569.
			string kinds = ServerSettingHolder.ServerSettings.competitionEventUsingRoomKind;
			if (string.IsNullOrEmpty(kinds))
			{
				kinds = "381,382,383,384,385,386,310,311,312,313,314,369,370,371,372,373,374";
			}
			string start = FormatPeriod("CompetitionEventPeriod_StartDateTime", "2026-01-01 00:00:00");
			string end = FormatPeriod("CompetitionEventPeriod_EndDateTime", "2099-12-31 23:59:59");
			ns.Write(1);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(start);
			ns.WriteAnsiFixed_intSize(end);
			ns.WriteAnsiFixed_intSize(kinds);
			_ = last;
		}

		private static string FormatPeriod(string key, string fallback)
		{
			string raw = fallback;
			try
			{
				foreach (var item in ServerSettingHolder.ServerSettingList)
				{
					if (item != null && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)
						&& !string.IsNullOrWhiteSpace(item.Value))
					{
						raw = item.Value.Trim();
						break;
					}
				}
			}
			catch
			{
			}
			DateTime dt;
			if (GameDate.TryParse(raw, out dt))
			{
				return GameDate.Format(dt);
			}
			return fallback;
		}
	}
}
