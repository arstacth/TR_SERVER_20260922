using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ReloadFarmTypeNum : NetPacket
	{
		public ReloadFarmTypeNum(int FarmUniqueNum, int FarmTypeNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ReloadFarmMapInfo_ACK_3);
			ns.Write(0);
			ns.Write(Utility.PackFarmUnique(FarmUniqueNum));
			// Type 1 here is farm_65536.trv. ENTER/GetMyFarmInfo keep this slot 0.
			ns.Write(FarmTypeNum == 1 || FarmTypeNum < 0 ? 0 : FarmTypeNum);
			_ = last;
		}
	}
}
