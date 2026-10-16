using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class InitProposeInfoACK : NetPacket
	{
		public InitProposeInfoACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_INIT_RECV_PROPOSE_INFO_ACK);
			_ = last;
		}
	}
}
