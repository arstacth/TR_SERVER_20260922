using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class PlayerFishedItem : NetPacket
	{
		public PlayerFishedItem(Account User, int itemid, int size, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FISHING_CATCH_FISH_NOTIFY);
			ns.Write(User.RoomPos);
			ns.Write((byte)1);
			ns.Write(itemid);
			ns.Write(size);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
