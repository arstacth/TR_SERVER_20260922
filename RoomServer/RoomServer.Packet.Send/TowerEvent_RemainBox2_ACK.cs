using System.Collections.Concurrent;
using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.TowerEvent;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_RemainBox2_ACK : NetPacket
	{
		public TowerEvent_RemainBox2_ACK(ConcurrentDictionary<int, Tower_BoxData> BoxList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_OREDEAL_ITEMINDEX_NOTIFY);
			ns.Write(BoxList.Count);
			foreach (KeyValuePair<int, Tower_BoxData> Box in BoxList)
			{
				ns.Write(Box.Key);
				ns.Write(Box.Value.QuizType);
				ns.Write(Box.Value.BoxGrade);
				ns.Write(Box.Value.QuestionNum);
				ns.Write(Box.Value.UNK2);
				ns.WriteBIG5Fixed_shortSize(Box.Value.Question);
				ns.WriteBIG5Fixed_shortSize(Box.Value.Answer);
			}
			_ = last;
		}
	}
}
