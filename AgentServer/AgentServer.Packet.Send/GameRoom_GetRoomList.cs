using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 1059 from debug_trgame 0x69EB28 / row 0x72AD90:
	/// roomkind + maxPage + page + count(u8) + type0 rows.
	/// </summary>
	public sealed class GameRoom_GetRoomList : NetPacket
	{
		public GameRoom_GetRoomList(List<NormalRoom> rooms, int roomkindid, int page, byte getCount, byte last)
		{
			int pageSize = (roomkindid == 79) ? 6 : 16;
			if (getCount > 0 && getCount < 64)
			{
				pageSize = getCount;
			}
			List<NormalRoom> source = rooms ?? new List<NormalRoom>();
			if (roomkindid == 0 && source.Count == 0)
			{
				source = Rooms.RoomList.Values
					.Where((NormalRoom r) => r.RoomKindID != 75 && r.RoomKindID != 76)
					.ToList();
			}
			int maxPage = Convert.ToInt32(Math.Ceiling(source.Count / (double)pageSize)) - 1;
			if (maxPage < 0)
			{
				maxPage = 0;
			}
			if (page < 0)
			{
				page = 0;
			}
			if (page > maxPage)
			{
				page = maxPage;
			}
			List<NormalRoom> pageRows = source.Skip(page * pageSize).Take(pageSize).ToList();
			ns.WriteOP(Opcodes.eServer_ROOM_LIST_ACK);
			ns.Write(roomkindid);
			ns.Write(maxPage);
			ns.Write(page);
			ns.Write((byte)pageRows.Count);
			foreach (NormalRoom item in pageRows)
			{
				ns.Write((byte)0);
				ns.Write(item.ID);
				ns.WriteAnsiFixed_intSize(item.Name ?? string.Empty);
				ns.WriteAnsiFixed_intSize(string.Empty);
				ns.Write(!item.HasPassword);
				ns.Write(item.PlayerCount);
				ns.Write(item.SlotCount);
				ns.Write(!item.isPlaying);
				ns.Write(item.IsStepOn);
				ns.Write((byte)0);
				ns.Write(item.ItemType);
				ns.Write(item.MapNum);
				ns.Write((item.IsTeamPlay == 2) ? (byte)1 : (byte)0);
				ns.Write(false);
				ns.Write(false);
				ns.Write(item.GMItem);
				ns.Write((byte)0);
				ns.Write((byte)0);
				ns.Write((byte)1);
				ns.Write(6);
				ns.Write(-1);
				ns.Write(item.BuffType != 0);
				ns.Write((int)item.BuffType);
				ns.Write(item.ItemNum);
				if (roomkindid == 79 || item.RoomKindID == 79)
				{
					ns.Write(0);
					ns.Write(item.GuildMatchRoomID);
					ns.Write(0L);
				}
			}
			_ = last;
		}
	}
}
