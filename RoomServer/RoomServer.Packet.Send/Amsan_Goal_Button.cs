using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Amsan_Goal_Button : NetPacket
	{
		public Amsan_Goal_Button(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_END_GAME_BONUS_ACK);
			ns.Write((short)0);
			_ = last;
		}
	}
}
