using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class IncreaseAnimalSizeOK : NetPacket
	{
		public IncreaseAnimalSizeOK(long FarmItemID, int View, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.IncreaseAnimalSize_ACK);
			ns.Write(0);
			ns.Write(FarmItemID);
			ns.Write(View);
			_ = last;
		}
	}
}
