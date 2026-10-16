using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RestoreAnimalDefaultSizeOK : NetPacket
	{
		public RestoreAnimalDefaultSizeOK(long FarmItemID, int View, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.RestoreAnimalDefaultSize_ACK);
			ns.Write(0);
			ns.Write(FarmItemID);
			ns.Write(View);
			_ = last;
		}
	}
}
