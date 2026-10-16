using AgentServer.Structuring.Attendance;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Attendance_Reward_ACK : NetPacket
	{
		public Attendance_Reward_ACK(int error, int attendance_key, AttendanceReward reward, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ATTENDANCE_REWARD_ACK);
			ns.Write(error);
			ns.Write(attendance_key);
			ns.Write(reward?.AttendanceRewardIndex ?? 0);
			ns.Write(reward?.Item ?? 0);
			_ = last;
		}
	}
}
