using System.Collections.Generic;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FarmItemList : NetPacket
	{
		public FarmItemList(List<FarmItem> farmitemlist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetFarmItemList_ACK);
			ns.Write(farmitemlist.Count);
			foreach (FarmItem item in farmitemlist)
			{
				ns.Write(item.FarmItemID);
				ns.Write(item.ItemDescNum);
				ns.Write(item.Exp);
				ns.Write(item.View);
				ns.Write(559370416);
				ns.Write(item.ExpireDateTime);
			}
			_ = last;
		}
	}
}
