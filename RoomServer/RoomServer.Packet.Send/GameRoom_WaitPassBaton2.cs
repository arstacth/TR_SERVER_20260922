using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_WaitPassBaton2 : NetPacket
	{
		public GameRoom_WaitPassBaton2(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ENTER_BATON_TOUCH_AREA_ACK);
			ns.Write(User.RoomPos);
			_ = last;
		}
	}
}
