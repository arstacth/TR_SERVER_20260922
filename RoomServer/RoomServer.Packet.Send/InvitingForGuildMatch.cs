using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class InvitingForGuildMatch : NetPacket
	{
		public InvitingForGuildMatch(NormalRoom room, short status, bool master, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILDMATCH_MATCH_ACTION_NOTIFY);
			ns.Write(status);
			ns.Write(2);
			ns.WriteAnsiFixed_intSize(room.PlayerList().Find((Account f) => f.RoomPos == room.RoomMasterIndex).NickName);
			ns.Write(room.GuildMatchRoomID);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(room.Name);
			ns.Write(room.GuildInfo.guildNum);
			ns.WriteAnsiFixed_intSize(room.GuildInfo.guildName);
			ns.Write(0);
			ns.Write(room.ID);
			ns.Write(1);
			ns.Write(0);
			ns.Write((short)0);
			ns.Write((short)1);
			ns.Write((short)room.GuildInfo.level);
			ns.Write(0);
			ns.Write(-1);
			ns.Write(master);
			_ = last;
		}
	}
}
