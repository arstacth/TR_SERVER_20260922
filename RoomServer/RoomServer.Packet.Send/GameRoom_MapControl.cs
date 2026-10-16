using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MapControl : NetPacket
	{
		public GameRoom_MapControl(short len, byte[] unk, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FORWARD_TO_ALL_ROOM_USER_ACK);
			ns.Write(len);
			ns.Write(unk, 0, unk.Length);
			_ = last;
		}
	}
}
