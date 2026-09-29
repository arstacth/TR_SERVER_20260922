using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Bomb_CountingStartAck : NetPacket
	{
		public Bomb_CountingStartAck(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BOMB_COUTING_START_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
