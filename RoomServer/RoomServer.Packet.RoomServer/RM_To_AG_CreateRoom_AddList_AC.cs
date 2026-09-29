using System.Linq;
using LocalCommons.Network;
using NetMsg.Room;
using RoomServer.Structuring;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_CreateRoom_AddList_ACK : NetPacket
	{
		public RM_To_AG_CreateRoom_AddList_ACK(NormalRoom room)
		{
			_ = room.Name;
			_ = room.Password;
			_ = room.IsTeamPlay;
			_ = room.IsStepOn;
			_ = room.ItemType;
			ns.WriteOP(RMProtocol.RM_CreateRoom_ACK);
			bool value = room.Players.Values.Any((Account player) => player.Attribute == 1);
			bool value2 = room.Players.Values.Any((Account player) => player.Attribute == 3);
			ns.Write(ServerStatus.MyRoomServerID);
			ns.Write(room.ID);
			// Event kinds are 310–386 (official summer enter=381). byte truncates 381→125.
			ns.Write(room.RoomKindID);
			ns.WriteAnsiFixed_intSize(room.Name);
			ns.Write(!room.HasPassword);
			ns.Write((byte)room.PlayerCount());
			ns.Write(room.SlotCount);
			ns.Write(!room.isPlaying);
			ns.Write(room.IsStepOn);
			ns.Write(room.ItemType);
			ns.Write(room.MapNum);
			ns.Write(6);
			ns.Write(room.IsTeamPlay);
			ns.Write(value2);
			ns.Write(value);
			ns.Write(room.GMItem);
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write((byte)1);
			ns.Write(-1);
			ns.Write(room.BuffType);
			ns.Fill(3);
			ns.Write(room.ItemNum);
			ns.Write(0);
			ns.Write(room.FarmIndex);
			if (room.RoomKindID == 79)
			{
				ns.WriteAnsiFixed_intSize(room.GuildInfo.guildName);
				ns.Write(0);
				ns.Write(room.GuildMatchRoomID);
				ns.Write(1);
			}
		}
	}
}
