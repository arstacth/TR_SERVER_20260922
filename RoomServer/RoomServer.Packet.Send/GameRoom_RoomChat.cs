using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomChat : NetPacket
	{
		public GameRoom_RoomChat(byte pos, byte[] text, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOM_CHATTING);
			ns.Write(pos);
			ns.Write(text, 0);
			_ = last;
		}
	}
}
