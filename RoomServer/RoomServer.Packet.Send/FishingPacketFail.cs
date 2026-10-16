using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FishingPacketFail : NetPacket
	{
		public FishingPacketFail(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_PROC_FISHING_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
