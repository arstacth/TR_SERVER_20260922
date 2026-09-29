using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FootStep_GoalIn : NetPacket
	{
		public FootStep_GoalIn(byte pos, int unk, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_STEPPED_GOAL_BOARD_ACK);
			ns.Write(pos);
			ns.Write(unk);
			_ = last;
		}
	}
}
