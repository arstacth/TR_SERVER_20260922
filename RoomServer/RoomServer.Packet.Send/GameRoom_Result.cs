using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_Result : NetPacket
	{
		public GameRoom_Result(int roomkind, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_RECEIVE_ROOM_RESULT_FOR_MISSION_CHECK_NOTIFY);
			ns.Write((byte)0);
			ns.Write(roomkind);
			_ = last;
		}
	}
}
