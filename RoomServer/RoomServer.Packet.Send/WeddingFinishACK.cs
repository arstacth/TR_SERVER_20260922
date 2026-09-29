using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class WeddingFinishACK : NetPacket
	{
		public WeddingFinishACK(long farmitemid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_WEDDING_CREATE_COUPLE_ACK);
			ns.Write((byte)0);
			ns.Write(farmitemid);
			_ = last;
		}
	}
}
