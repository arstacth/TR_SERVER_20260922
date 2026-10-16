using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TypingRun_GoblinRacingTypingDone : NetPacket
	{
		public TypingRun_GoblinRacingTypingDone(int index, int rank, int total_user, int point, int msec, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_TYPEINGRUN_GOBLINRACING_TYPING_DONE_ACK);
			ns.Write(index);
			ns.Write(rank);
			ns.Write(total_user);
			ns.Write(point);
			ns.Write(msec);
			_ = last;
		}
	}
}
