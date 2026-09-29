using System.Collections.Generic;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetGuildMissionList_ACK : NetPacket
	{
		public GetGuildMissionList_ACK(List<GuildMissionInfo> info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_MISSION_GET_USER_GUILD_MISSION_ACK);
			ns.Write(info.Count);
			foreach (GuildMissionInfo item in info)
			{
				ns.Write(item.missionNum);
				ns.Write(item.isMaster);
				ns.Fill(3);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
