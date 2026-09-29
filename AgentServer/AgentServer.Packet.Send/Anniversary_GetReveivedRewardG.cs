using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_GetReveivedRewardGradeList_ACK : NetPacket
	{
		public Anniversary_GetReveivedRewardGradeList_ACK(int iObjectNum, long iValue, List<byte> grades, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANNIVERSARY_GET_RECEIVED_REWARD_GRADE_LIST_ACK);
			ns.Write(iValue);
			ns.Write(iObjectNum);
			ns.Write(grades.Count);
			foreach (byte grade in grades)
			{
				ns.Write(grade);
			}
			_ = last;
		}
	}
}
