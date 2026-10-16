using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ChangeGuildMatchRoomName : NetPacket
	{
		public GameRoom_ChangeGuildMatchRoomName(string name, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOM_OPTION_MODIFY_TITLE_ACK);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
