using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Amsan_LapTime : NetPacket
	{
		public Amsan_LapTime(int unk, byte round, byte pos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOM_EVENT);
			ns.Write(unk);
			ns.Write(pos);
			ns.Write(round);
			_ = last;
		}
	}
}
