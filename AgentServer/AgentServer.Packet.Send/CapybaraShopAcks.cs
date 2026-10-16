using System.Collections.Generic;
using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Packed wire 78. Empty body (result+count) parsed RemainSize=1 PacketSize=11.
	/// Live Overpop 10 39 32 78: after opcode+result+count the client popRawData(32)
	/// per row. Wire struct is 32 bytes (short displayStand + 6-byte pad before
	/// the two int64 times). Writing displayStand as int then an extra 0 matches that.
	/// </summary>
	public sealed class CapybaraShopTodayScheduleAck : NetPacket
	{
		public CapybaraShopTodayScheduleAck(IList<CapybaraScheduleRow> rows, byte last)
		{
			ns.Write((ushort)78);
			ns.Write(0);
			int count = rows != null ? rows.Count : 0;
			ns.Write(count);
			if (rows != null)
			{
				for (int i = 0; i < count; i++)
				{
					CapybaraScheduleRow row = rows[i];
					ns.Write(row.ScheduleNum);
					ns.Write(row.ShopNum);
					ns.Write(row.DisplayStandNum);
					ns.Write(0);
					// Client reads open/close as signed int32 (+ 4-byte pad). Dates past 2038
					// (e.g. 2099-12-31) overflow to negative garbage in dbgtrace.
					ns.Write(ClampUnixSeconds32(row.OpenTime));
					ns.Write(0);
					ns.Write(ClampUnixSeconds32(row.CloseTime));
					ns.Write(0);
				}
			}
			_ = last;
		}

		private static int ClampUnixSeconds32(long timeMs)
		{
			long sec = timeMs / 1000L;
			if (sec < 0L)
			{
				return 0;
			}
			if (sec > int.MaxValue)
			{
				return int.MaxValue;
			}
			return (int)sec;
		}
	}

	/// <summary>
	/// Packed wire 1300. Login never sent this; the village icon stays closed
	/// without an open notify. Two ints (shop, isOpen) matches empty-ACK size 11.
	/// </summary>
	public sealed class CapybaraShopOpenCloseNotify : NetPacket
	{
		public CapybaraShopOpenCloseNotify(int shopNum, int isOpen, byte last)
		{
			ns.Write((ushort)1300);
			ns.Write(shopNum);
			ns.Write(isOpen);
			_ = last;
		}
	}

	/// <summary>
	/// Packed wire 1508. User trade counts for one shop/schedule.
	/// </summary>
	public sealed class CapybaraShopUserInfoAck : NetPacket
	{
		public CapybaraShopUserInfoAck(int shopNum, int scheduleNum, IList<CapybaraUserTradeRow> rows, byte last)
		{
			ns.Write((ushort)1508);
			ns.Write(1);
			ns.Write(shopNum);
			ns.Write(scheduleNum);
			int count = rows != null ? rows.Count : 0;
			ns.Write(count);
			if (rows != null)
			{
				for (int i = 0; i < count; i++)
				{
					CapybaraUserTradeRow row = rows[i];
					ns.Write(row.ItemNum);
					ns.Write(row.Category);
					ns.Write(row.TradeCount);
				}
			}
			_ = last;
		}
	}

	/// <summary>
	/// Packed wire 591. result + accumulated trade count from usp_CapybaraShop_TradeItem.
	/// </summary>
	public sealed class CapybaraShopTradeAck : NetPacket
	{
		public CapybaraShopTradeAck(int result, int tradeCount, byte last)
		{
			ns.Write((ushort)591);
			ns.Write(result);
			ns.Write(tradeCount);
			_ = last;
		}
	}
}
