using System;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TypingRun_GoblinRacingQuestion : NetPacket
	{
		public TypingRun_GoblinRacingQuestion(int index, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TYPEINGRUN_GOBLINRACING_QUESTION_ACK);
			ns.Write(index);
			ns.Write(index);
			for (int i = 1; i <= index; i++)
			{
				int value = new Random(Guid.NewGuid().GetHashCode()).Next(4);
				ns.Write(value);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
