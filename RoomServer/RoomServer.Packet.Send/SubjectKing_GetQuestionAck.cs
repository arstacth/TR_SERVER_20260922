using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SubjectKing_GetQuestionAck : NetPacket
	{
		public SubjectKing_GetQuestionAck(List<int> questionindexs, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SUBJECTKING_QUESTION_ACK);
			ns.Write(4);
			foreach (int questionindex in questionindexs)
			{
				ns.Write(questionindex);
				ns.Write(1);
			}
			_ = last;
		}
	}
}
