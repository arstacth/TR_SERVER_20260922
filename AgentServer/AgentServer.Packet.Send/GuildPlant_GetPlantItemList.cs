using System.Collections.Generic;
using AgentServer.Structuring.GuildPlant;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetPlantItemList : NetPacket
	{
		public GuildPlant_GetPlantItemList(List<GuildPlantSellInfo> useList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_PLANT_ITEM_LIST_ACK);
			ns.Write(useList.Count);
			foreach (GuildPlantSellInfo use in useList)
			{
				ns.Write(use.SellNum);
				ns.Write(0);
				ns.Write(use.ItemDescNum);
				ns.Write(use.PointType);
				ns.Write(use.PointValue);
				ns.Write(use.MaxCount);
				ns.Write(use.BuyCount);
				ns.Write(use.FinishDate);
			}
			_ = last;
		}
	}
}
