using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_UserState : NetPacket
	{
		public GameRoom_UserState(byte pos, byte[] array, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FORWARD_TO_ROOM_USER_ACK);
			ns.Write(pos);
			ns.Write(array, 0);
			_ = last;
		}
	}
}
