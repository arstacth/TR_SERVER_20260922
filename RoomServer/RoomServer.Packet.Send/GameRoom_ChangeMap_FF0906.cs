using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ChangeMap_FF0906 : NetPacket
	{
		public GameRoom_ChangeMap_FF0906(int mapid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SET_MEDAL);
			ns.Write(mapid);
			_ = last;
		}
	}
}
