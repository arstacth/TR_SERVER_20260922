using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Bomb_RaceTransferBombAck : NetPacket
	{
		public Bomb_RaceTransferBombAck(int from_user, int to_user, int left_time, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BOMB_RACE_TRANSFER_BOMB_ACK);
			ns.Write(from_user);
			ns.Write(to_user);
			ns.Write(left_time);
			ns.Write(0);
			_ = last;
		}
	}
}
