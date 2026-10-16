using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserMissionInfo_ACK : NetPacket
	{
		public GetUserMissionInfo_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_LIST_ACK);
			List<MissionInfo> list = (from o in User.Mission.MissionInfo.Concat(User.DailyMission.MissionInfo)
				orderby o.Key
				select o into s
				select s.Value).ToList();
			ns.Write(list.Count());
			foreach (MissionInfo item in list)
			{
				ns.Write(item.MissionKind);
				ns.Write(item.MissionNum);
				ns.Write(item.challengeState);
				ns.Write(146603208);
				ns.Write(item.challengeExpireTime);
				ns.Write(item.ConditionInfo.Count);
				foreach (KeyValuePair<int, MissionConditionInfo> item2 in item.ConditionInfo)
				{
					ns.Write(item2.Key);
					if (item.challengeState == 2 || item.challengeState == 5)
					{
						ns.Write(0);
					}
					else
					{
						ns.Write(item2.Value.achievedPoint);
					}
				}
			}
			_ = last;
		}
	}
}
