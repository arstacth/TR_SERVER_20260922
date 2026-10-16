using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class UseCoupleExpAddItemFail_ACK : NetPacket
	{
		public UseCoupleExpAddItemFail_ACK(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.Write((byte)65);
			ns.Write(err);
			ns.Write((short)0);
			_ = last;
		}
	}
}
