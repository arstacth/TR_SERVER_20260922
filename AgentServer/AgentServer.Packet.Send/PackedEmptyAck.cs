using System.Collections.Generic;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PackedEmptyAck : NetPacket
	{
		public PackedEmptyAck(ushort packedAckWire, byte last, int extraIntCount = 2)
			: this(packedAckWire, last, extraIntCount, writeLast: true, extraBytes: 0)
		{
		}

		public PackedEmptyAck(ushort packedAckWire, byte last, int extraIntCount, bool writeLast)
			: this(packedAckWire, last, extraIntCount, writeLast, extraBytes: 0)
		{
		}

		public PackedEmptyAck(ushort packedAckWire, byte last, int extraIntCount, bool writeLast, int extraBytes)
		{
			_ = last;
			ns.Write((ushort)packedAckWire);
			for (int i = 0; i < extraIntCount; i++)
			{
				ns.Write(0);
			}
			for (int i = 0; i < extraBytes; i++)
			{
				ns.Write((byte)0);
			}
			if (writeLast)
			{
				_ = last;
			}
		}
	}

	/// <summary>
	/// ENTRY_SYSTEM_SHOP_SCHEDULE_NOTIFY (1481).
	/// Wire: int + byte + long (trgame 0x762840). byte==1 means open.
	/// </summary>
	public sealed class Packed1481Ack : NetPacket
	{
		public Packed1481Ack(byte last)
			: this(0, last)
		{
		}

		public Packed1481Ack(int shopOrPeriodNum, byte last)
		{
			_ = last;
			ns.Write((ushort)1481);
			ns.Write(shopOrPeriodNum);
			ns.Write((byte)1);
			// End/open stamp far in the future (Gregorian-safe ms).
			ns.Write(LocalCommons.Utilities.Utility.CurrentTimeMilliseconds() + 86400000L * 365L * 50L);
		}
	}

	/// <summary>
	/// Packed wire 396. Client sends period id on wire 214 at lobby open.
	/// Without an open window the Special Event tab stays empty.
	/// </summary>
	public sealed class PackedEntrySystemPeriodNotify : NetPacket
	{
		public PackedEntrySystemPeriodNotify(int period, long openTime, long closeTime, byte last)
		{
			ns.Write((ushort)396);
			ns.Write(period);
			ns.Write(1);
			ns.Write(openTime);
			ns.Write(closeTime);
			// Size 30 Remain 3: extra int was too much; one byte fills Overpop 23 26 4.
			ns.Write((byte)0);
			_ = last;
		}
	}

	public sealed class PackedItemCollectionAck : NetPacket
	{
		public PackedItemCollectionAck(int value, byte last)
		{
			ns.Write((ushort)2177);
			ns.Write(value);
			_ = last;
		}

		public PackedItemCollectionAck(int rank, int point, byte last)
		{
			_ = rank;
			ns.Write((ushort)2177);
			ns.Write(point);
			_ = last;
		}
	}

	/// <summary>
	/// ItemCollection USER_INFO (inner 3). First int after inner is status;
	/// non-zero triggers OnRecvItemCollection_Failed. Do not append TCP last.
	/// Must keep kind + mode-byte + userNum(long) before point fields — omitting
	/// them Overpops (39/46) and My Info stays at 99999.
	/// </summary>
	public sealed class PackedItemCollectionUserInfoAck : NetPacket
	{
		public PackedItemCollectionUserInfoAck(long userNum, int monthPoint, int totalPoint, int heldPoint, byte noticedLevel, int rank, byte last)
			: this(userNum, monthPoint, totalPoint, heldPoint, 0, noticedLevel, rank, last)
		{
		}

		public PackedItemCollectionUserInfoAck(long userNum, int monthPoint, int totalPoint, int heldPoint, int itemCount, byte noticedLevel, int rank, byte last)
		{
			_ = itemCount;
			_ = last;
			int total = totalPoint > 0 ? totalPoint : heldPoint;
			int month = monthPoint > 0 ? monthPoint : total;
			ns.Write((ushort)2177);
			ns.Write(3);
			ns.Write(0);
			ns.Write(0);
			ns.Write((byte)1);
			// My Info level bar reads the 8 bytes after mode as points (low int).
			// userNum=1 made UI show 1/150 (Rookie). Put total points here instead.
			_ = userNum;
			ns.Write((long)total);
			ns.Write(month);
			ns.Write(total);
			ns.Write(total);
			ns.Write(noticedLevel);
			// Live 52-byte USER_INFO: rank sits after noticedLevel, then 3 pad ints.
			// 48-byte packet Overpop +4; 56-byte left Remain 8.
			ns.Write(rank > 0 ? rank : (total > 0 ? 1 : 0));
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
		}
	}

	/// <summary>
	/// ItemCollection MapAddItemNotify (inner 10). CollectionNum table
	/// rejects DB fdIDs — do not send on open until ids match client data.
	/// </summary>
	public sealed class PackedItemCollectionOwnedListAck : NetPacket
	{
		public PackedItemCollectionOwnedListAck(IList<int> itemNums, byte last)
		{
			_ = last;
			ns.Write((ushort)2177);
			ns.Write(10);
			ns.Write(0);
			int count = (itemNums != null) ? itemNums.Count : 0;
			ns.Write(count);
			if (itemNums != null)
			{
				for (int i = 0; i < count; i++)
				{
					ns.Write(itemNums[i]);
				}
			}
		}
	}

	/// <summary>
	/// ItemCollection month/theme value (inner 5): status + one int.
	/// </summary>
	public sealed class PackedItemCollectionValueAck : NetPacket
	{
		public PackedItemCollectionValueAck(int value, byte last)
		{
			_ = last;
			ns.Write((ushort)2177);
			ns.Write(5);
			ns.Write(0);
			ns.Write(value);
		}
	}

	/// <summary>
	/// ItemCollection level/month reward claim (REQ inner 4, 7-byte). Echoing USER_INFO
	/// reopened LuckyBagItemCollectionLevelUpReward.gui (purple) with empty slots.
	/// ACK inner 4: status 0 + empty reward count closes the popup.
	/// </summary>
	public sealed class PackedItemCollectionRewardAck : NetPacket
	{
		public PackedItemCollectionRewardAck(byte last)
		{
			_ = last;
			ns.Write((ushort)2177);
			ns.Write(4);
			ns.Write(0);
			ns.Write(0);
		}
	}
}
