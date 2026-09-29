using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GetRoomList : NetPacket
	{
		public GameRoom_GetRoomList(List<NormalRoom> rooms, byte roomkindid, int page, byte last)
		{
			int num = ((roomkindid == 79) ? 6 : 16);
			IEnumerable<NormalRoom> enumerable = rooms.Skip(page * num).Take(num);
			int value = Convert.ToInt32(Math.Ceiling((double)rooms.Count / Convert.ToDouble(num))) - 1;
			if (value < 0)
			{
				value = 0;
			}
			ns.WriteOP(Opcodes.eServer_ROOM_LIST_ACK);
			ns.Write(roomkindid);
			ns.Write(value);
			ns.Write(page);
			ns.Write((short)enumerable.Count());
			foreach (NormalRoom item in enumerable)
			{
				bool value2 = item.Players.Values.Any((Account player) => player.Attribute == 1);
				bool value3 = item.Players.Values.Any((Account player) => player.Attribute == 3);
				ns.Write(item.ID);
				ns.WriteAnsiFixed_intSize(item.Name);
				ns.Write(!item.HasPassword);
				ns.Write((byte)item.PlayerCount());
				ns.Write(item.SlotCount);
				ns.Write(!item.isPlaying);
				ns.Write(item.IsStepOn);
				ns.Write(item.ItemType);
				ns.Write(item.MapNum);
				ns.Write(6);
				ns.Write((item.IsTeamPlay == 2) ? ((byte)1) : ((byte)0));
				ns.Write(value3);
				ns.Write(value2);
				ns.Write(item.GMItem);
				ns.Write((byte)0);
				ns.Write((byte)0);
				ns.Write((byte)1);
				ns.Write(-1);
				ns.Write(item.BuffType);
				ns.Fill(3);
				ns.Write(item.ItemNum);
				ns.Write(0);
				if (roomkindid == 79)
				{
					ns.WriteAnsiFixed_intSize(item.GuildInfo.guildName);
					ns.Write(0);
					ns.Write(item.GuildMatchRoomID);
					ns.Write(1);
				}
				ns.Write((byte)0);
			}
			ns.Seek(ns.Position - 1, SeekOrigin.Begin);
			_ = last;
		}
	}
}
