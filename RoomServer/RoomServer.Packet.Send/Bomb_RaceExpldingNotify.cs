using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Bomb_RaceExpldingNotify : NetPacket
	{
		public Bomb_RaceExpldingNotify(int user, int left_bomb_count, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BOMB_RACE_EXPLODING_NOTIFY);
			ns.Write(user);
			ns.Write(left_bomb_count);
			_ = last;
		}
	}
}
