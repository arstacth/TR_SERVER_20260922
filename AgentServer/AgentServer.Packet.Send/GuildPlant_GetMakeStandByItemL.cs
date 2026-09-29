using System.Collections.Generic;
using AgentServer.Structuring.GuildPlant;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetMakeStandByItemList : NetPacket
	{
		public GuildPlant_GetMakeStandByItemList(List<GuildPlantMakeInfo> makeInfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_MAKE_STAND_BY_ITEM_LIST_ACK);
			ns.Write(makeInfos.Count);
			foreach (GuildPlantMakeInfo makeInfo in makeInfos)
			{
				ns.Write(makeInfo.ItemIndexNum);
				ns.Write(makeInfo.ItemDescNum);
				ns.Write(makeInfo.NeedPoint);
				ns.Write(makeInfo.AccumulatePoint);
				ns.Write(Utility.ConvertToTimestamp(makeInfo.FinishDate));
				ns.Write(0);
			}
			_ = last;
		}
	}
}
