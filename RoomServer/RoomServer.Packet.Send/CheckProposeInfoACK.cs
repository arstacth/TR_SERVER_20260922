using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CheckProposeInfoACK : NetPacket
	{
		public CheckProposeInfoACK(int ret, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_CHECK_PROPOSE_INFO_ACK);
			ns.Write((byte)0);
			ns.Write(ret);
			_ = last;
		}
	}
}
