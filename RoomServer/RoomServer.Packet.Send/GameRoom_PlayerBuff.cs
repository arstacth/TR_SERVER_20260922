using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_PlayerBuff : NetPacket
	{
		public GameRoom_PlayerBuff(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SUPER_ROOM_MASTER_INFO_NOTIFY);
			ns.Write(room.RoomMasterIndex);
			ns.Write(0L);
			ns.Write(0.2f);
			_ = last;
		}
	}
}
