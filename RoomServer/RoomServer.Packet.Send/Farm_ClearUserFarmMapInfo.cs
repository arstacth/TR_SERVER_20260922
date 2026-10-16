using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Farm_ClearUserFarmMapInfo : NetPacket
	{
		public Farm_ClearUserFarmMapInfo(int FarmUniqueNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ClearUserFarmMapInfo_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(last);
		}
	}
}
