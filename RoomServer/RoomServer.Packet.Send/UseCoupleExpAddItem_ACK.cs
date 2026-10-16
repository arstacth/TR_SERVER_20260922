using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class UseCoupleExpAddItem_ACK : NetPacket
	{
		public UseCoupleExpAddItem_ACK(int itemnum, int totalexp, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.Write((byte)64);
			ns.Write((byte)1);
			ns.Write(itemnum);
			ns.Write(totalexp);
			ns.Write(0);
			_ = last;
		}
	}
}
