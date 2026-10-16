using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class WeddingSuitForDivorceACK : NetPacket
	{
		public WeddingSuitForDivorceACK(int err, int DivorceType, byte last)
		{
			ns.WriteOP(Opcodes.eServer_WEDDING_SUIT_FOR_DIVORCE_ACK);
			ns.Write(err);
			ns.Write(DivorceType);
			_ = last;
		}
	}
}
