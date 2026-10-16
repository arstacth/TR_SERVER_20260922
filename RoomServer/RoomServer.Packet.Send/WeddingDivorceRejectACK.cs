using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class WeddingDivorceRejectACK : NetPacket
	{
		public WeddingDivorceRejectACK(int DisagreeType, byte last)
		{
			ns.WriteOP(Opcodes.eServer_WEDDING_INIT_DIVORCE_REQUEST_INFO_ACK);
			ns.Write(DisagreeType);
			_ = last;
		}
	}
}
