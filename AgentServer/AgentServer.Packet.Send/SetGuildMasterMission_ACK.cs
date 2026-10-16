using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SetGuildMasterMission_ACK : NetPacket
	{
		public SetGuildMasterMission_ACK(List<int> info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_MISSION_SET_MASTER_MISSION_ACK);
			ns.Write(info.Count);
			foreach (int item in info)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
