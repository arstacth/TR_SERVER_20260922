using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Attendance;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Attendance_Info_ACK : NetPacket
	{
		public Attendance_Info_ACK(Dictionary<int, AttendanceInfo> attendanceInfos, List<AttendanceReward> attendanceRewards, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ATTENDANCE_INFO_ACK);
			ns.Write((byte)0);
			ns.Write(attendanceInfos.Count);
			foreach (AttendanceInfo i in attendanceInfos.Values)
			{
				ns.Write(i.AttendanceKey);
				ns.Write(i.AttendanceType);
				ns.WriteBIG5Fixed_shortSize(string.Empty);
				ns.WriteBIG5Fixed_shortSize(string.Empty);
				ns.Write(i.Start);
				ns.Write(i.EndS);
				ns.Write(i.EndU);
				ns.Write(i.AttendancePoint);
				ns.Write(new long?(i.userAttendanceInfos.LastDate).GetValueOrDefault());
				ns.Write(attendanceRewards.Count((AttendanceReward c) => c.AttendanceRewardGroupKey == i.AttendanceRewardGroupKey));
				foreach (AttendanceReward item in from c in attendanceRewards
					where c.AttendanceRewardGroupKey == i.AttendanceRewardGroupKey
					select c into o
					orderby o.AttendanceRewardIndex
					select o)
				{
					ns.Write(item.Item);
					ns.Write(new int?(i.userAttendanceInfos.AttendanceRewardIndex).GetValueOrDefault() >= item.AttendanceRewardIndex);
				}
			}
			_ = last;
		}
	}
}
