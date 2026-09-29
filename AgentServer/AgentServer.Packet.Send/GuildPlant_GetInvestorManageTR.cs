using System;
using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetInvestorManageTRList : NetPacket
	{
		public GuildPlant_GetInvestorManageTRList(List<Tuple<int, string>> userList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_INVESTOR_MANAGE_TR_LIST_ACK);
			ns.Write(userList.Count);
			foreach (Tuple<int, string> user in userList)
			{
				ns.Write(user.Item1);
				ns.WriteBIG5Fixed_shortSize(user.Item2);
			}
			_ = last;
		}
	}
}
