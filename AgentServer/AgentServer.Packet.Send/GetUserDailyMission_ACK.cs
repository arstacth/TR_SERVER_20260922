using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserDailyMission_ACK : NetPacket
	{
		public GetUserDailyMission_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_GET_USER_ONEDAY_MISSION_ACK);
			ns.Write(User.DailyMission.MissionInfo.Count);
			foreach (MissionInfo item in User.DailyMission.MissionInfo.Values.OrderBy((MissionInfo o) => o.MissionNum))
			{
				ns.Write(item.MissionNum);
				ns.Write(item.MissionKind);
				ns.Write(122944768);
			}
			ns.Write(User.DailyMission.FinishedInfo.MissionFinishedInfo.Count);
			foreach (KeyValuePair<int, int> item2 in User.DailyMission.FinishedInfo.MissionFinishedInfo)
			{
				ns.Write(item2.Key);
				ns.Write((byte)item2.Value);
			}
			_ = last;
		}
	}
}
