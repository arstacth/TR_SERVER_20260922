using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DailyMissionUserMissionDeleteNotify_ACK : NetPacket
	{
		public DailyMissionUserMissionDeleteNotify_ACK()
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_USER_MISSION_DELETE_NOTIFY);
			ns.Write(4);
			ns.Write(3);
			ns.Write(5);
			ns.Write(4);
			ns.Write(6);
			ns.Write(0);
			// Live Remain=1 size=27 — drop trailing byte(1).
		}
	}
}
