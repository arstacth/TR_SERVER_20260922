using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class WeddingReadyACK : NetPacket
	{
		public WeddingReadyACK(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_WEDDING_AGREE_ACK);
			ns.Write(124);
			_ = last;
		}
	}
}
