using System;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_LockKeepItem : NetPacket
	{
		public GameRoom_LockKeepItem(NormalRoom room, bool isCancel, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_ITEM_UPDATE);
			ns.Write(1);
			ns.Write(2L);
			ns.Write(room.Storage_Id);
			ns.Write(room.ItemNum);
			ns.Write(Utility.ConvertToTimestamp(DateTime.Now));
			ns.Write(1);
			ns.Write(1);
			ns.Write(isCancel ? 0f : 1f);
			ns.Write(0);
			_ = last;
		}
	}
}
