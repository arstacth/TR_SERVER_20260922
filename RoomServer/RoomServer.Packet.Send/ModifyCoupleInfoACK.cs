using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ModifyCoupleInfoACK : NetPacket
	{
		public ModifyCoupleInfoACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_MODIFY_COUPLE_INFO_ACK);
			_ = last;
		}
	}
}
