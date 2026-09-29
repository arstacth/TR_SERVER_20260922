using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RemoveCoupleACK : NetPacket
	{
		public RemoveCoupleACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_REMOVE_COUPLE_INFO_ACK);
			_ = last;
		}
	}
}
