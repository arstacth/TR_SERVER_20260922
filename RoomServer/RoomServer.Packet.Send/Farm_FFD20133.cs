using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Farm_FFD20133 : NetPacket
	{
		public Farm_FFD20133(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ReloadFarmMapInfo_ACK_1);
			_ = last;
		}
	}
}
