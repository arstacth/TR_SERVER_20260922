using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AnubisUserPoint : NetPacket
	{
		public AnubisUserPoint(int point, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANUBIS_EXPEDITION_GET_USER_INFO_ACK);
			ns.Write(point);
			_ = last;
		}
	}
}
