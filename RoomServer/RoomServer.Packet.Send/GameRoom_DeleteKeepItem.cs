using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_DeleteKeepItem : NetPacket
	{
		public GameRoom_DeleteKeepItem(Account User, NormalRoom room, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_ITEM_UPDATE);
			ns.Write(1);
			ns.Write(1L);
			ns.Write(room.Storage_Id);
			_ = last;
		}
	}
}
