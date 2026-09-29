using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ChangeMap_FF4E03 : NetPacket
	{
		public GameRoom_ChangeMap_FF4E03(Account User, int mapid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MAP_CHANGE_ACK);
			ns.Write(mapid);
			_ = last;
		}
	}
}
