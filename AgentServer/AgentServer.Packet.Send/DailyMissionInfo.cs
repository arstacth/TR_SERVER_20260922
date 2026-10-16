using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DailyMissionInfo : NetPacket
	{
		public DailyMissionInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_GET_USER_ONEDAY_MISSION_ACK);
			ns.Write(0L);
			_ = last;
		}
	}
}
