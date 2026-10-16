using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class OneDayMissionReload_ACK : NetPacket
	{
		public OneDayMissionReload_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_GROUP_ADD_ACK);
			ns.Write(User.DailyMission.MissionInfo.Count);
			foreach (MissionInfo item in User.DailyMission.MissionInfo.Values.OrderBy((MissionInfo o) => o.MissionNum))
			{
				ns.Write(item.MissionKind);
				ns.Write(item.MissionNum);
				ns.Write(item.challengeExpireTime);
			}
			_ = last;
		}
	}
}
