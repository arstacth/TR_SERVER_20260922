using System.Collections.Generic;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UpdateMission_ACK : NetPacket
	{
		public UpdateMission_ACK(List<MissionConditionInfo> mcinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_UPDATE_ACK);
			ns.Write(mcinfos.Count);
			foreach (MissionConditionInfo mcinfo in mcinfos)
			{
				ns.Write(mcinfo.missionNum);
				ns.Write(mcinfo.conditionNum);
				ns.Write(mcinfo.achievedPoint);
			}
			_ = last;
		}
	}
}
