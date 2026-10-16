using System.Collections.Generic;
using AgentServer.Structuring.GuildPlant;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetExpenseList : NetPacket
	{
		public GuildPlant_GetExpenseList(int pointType, int month, List<GuildPlantPointUseInfo> useList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_EXPENSE_LIST_ACK);
			ns.Write(pointType);
			ns.Write(month);
			ns.Write(useList.Count);
			foreach (GuildPlantPointUseInfo use in useList)
			{
				ns.Write(Utility.ConvertToTimestamp(use.DateTime));
				ns.Write(use.UsePoint);
				ns.WriteBIG5Fixed_shortSize(use.Memo);
				ns.WriteBIG5Fixed_shortSize(use.NickName);
				ns.Write(use.RemainPoint);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
