using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DailyMissionFinishedInfo_ACK : NetPacket
	{
		public DailyMissionFinishedInfo_ACK(int kind, DailyMissionFinishedInfo Info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_GET_USER_MISSION_FINISH_COUNT_ACK);
			ns.Write(1);
			ns.Write(kind);
			ns.Write(Info.MissionFinishedInfo[kind]);
			_ = last;
		}
	}
}
