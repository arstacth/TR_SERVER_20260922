using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class eServer_COUPLE_CREATE_COUPLE_INFO_ACK : NetPacket
	{
		public eServer_COUPLE_CREATE_COUPLE_INFO_ACK(int coupleNum, int coupleRingNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_CREATE_COUPLE_INFO_ACK);
			ns.Write((byte)0);
			ns.Write(coupleNum);
			ns.Write(coupleRingNum);
			_ = last;
		}
	}
}
