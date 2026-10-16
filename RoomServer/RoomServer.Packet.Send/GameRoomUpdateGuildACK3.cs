using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoomUpdateGuildACK3 : NetPacket
	{
		public GameRoomUpdateGuildACK3(byte pos, string name, string guildname, int guildnum, int grade, int unk, short unk2, byte last)
		{
			ns.WriteOP(Opcodes.eServer_UPDATE_GUILD_INFO_ACK);
			ns.Write(pos);
			ns.WriteAnsiFixed_intSize(name);
			ns.WriteAnsiFixed_intSize(guildname);
			ns.Write(guildnum);
			ns.Write(grade);
			ns.Write(unk);
			ns.Write(unk2);
			_ = last;
		}
	}
}
