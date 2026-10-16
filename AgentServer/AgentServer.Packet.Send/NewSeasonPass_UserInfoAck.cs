using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public struct NewSeasonPassRow
	{
		public int Kind;
		public int Group;
		public byte FreeLvl;
		public byte PlusLvl;
		public int Point;
		public int UsePeriod;
		public long StartMs;
		public long EndMs;
		public byte AccEnable;
	}

	/// <summary>
	/// Wire 1323 from debug_trgame 0x786220 / row 0x7914E0:
	/// discarded i32 + count i32 + rows:
	/// i32 i32 u8 u8 i64 i64 + subCount i32 + (u8 u8 u8)*subCount.
	/// No trailer.
	/// </summary>
	public sealed class NewSeasonPass_UserInfoAck : NetPacket
	{
		public NewSeasonPass_UserInfoAck(IList<NewSeasonPassRow> rows, byte last)
		{
			ns.Write((ushort)Opcodes.eServer_NEW_SEASON_PASS_USER_INFO_ACK);
			ns.Write(0);
			int count = rows != null ? rows.Count : 0;
			ns.Write(count);
			if (rows != null)
			{
				for (int i = 0; i < count; i++)
				{
					NewSeasonPassRow row = rows[i];
					ns.Write(row.Kind);
					ns.Write(row.Group);
					ns.Write(row.FreeLvl);
					ns.Write(row.PlusLvl);
					ns.Write(row.StartMs);
					ns.Write(row.EndMs);
					ns.Write(0);
				}
			}
			_ = last;
		}
	}

	/// <summary>
	/// Wire 1593. short partyType + int point per row.
	/// </summary>
	public sealed class CompetitionEvent_TeamPointAck : NetPacket
	{
		public CompetitionEvent_TeamPointAck(IList<KeyValuePair<int, int>> rows, byte last)
		{
			ns.Write((ushort)Opcodes.eServer_COMPETITION_EVENT_POINT_GATHERING_TEAM_POINT_ACK);
			int count = rows != null ? rows.Count : 0;
			ns.Write(count);
			if (rows != null)
			{
				for (int i = 0; i < count; i++)
				{
					ns.Write((short)rows[i].Key);
					ns.Write(rows[i].Value);
				}
			}
			_ = last;
		}
	}

	/// <summary>
	/// Wire 1513. Three ints.
	/// </summary>
	public sealed class PointGathering_MyInfoAck : NetPacket
	{
		public PointGathering_MyInfoAck(byte last)
		{
			ns.Write((ushort)Opcodes.eServer_POINT_GATHERING_MY_INFO_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
