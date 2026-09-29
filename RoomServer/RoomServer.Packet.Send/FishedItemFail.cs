using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FishedItemFail : NetPacket
	{
		public FishedItemFail(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_CATCH_FISH_NOTIFY);
			ns.Write(err);
			_ = last;
		}
	}
}
