using System.Collections.Generic;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ModifyFarmMapInfo : NetPacket
	{
		public ModifyFarmMapInfo(int FarmUniqueNum, List<FarmMapInfo> farmmapinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ModifyFarmMapInfo_ACK);
			ns.Write(Utility.PackFarmUnique(FarmUniqueNum));
			ns.Write(farmmapinfo.Count);
			foreach (FarmMapInfo item in farmmapinfo)
			{
				ns.Write((short)0);
				ns.Write(item.FarmItemID);
				ns.Write((short)item.ModifyType);
			}
			_ = last;
		}
	}
}
