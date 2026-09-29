using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetGivePossibleUserList : NetPacket
	{
		public GuildPlant_GetGivePossibleUserList(List<string> userList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_GIVE_POSSIBLE_USER_LIST_ACK);
			ns.Write(userList.Count);
			foreach (string user in userList)
			{
				ns.WriteBIG5Fixed_shortSize(user);
			}
			_ = last;
		}
	}
}
