using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartPassBaton : NetPacket
	{
		public GameRoom_StartPassBaton(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_LEAVE_BATON_TOUCH_AREA_ACK);
			ns.Write(User.RoomPos);
			_ = last;
		}
	}
}
