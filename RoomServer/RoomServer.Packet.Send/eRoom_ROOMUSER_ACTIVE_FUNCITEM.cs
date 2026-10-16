using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using TRCommon;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT : NetPacket
	{
		public eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT(Account User, CActiveItems deletedActiveItems, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOMUSER_ACTIVE_FUNCITEM_TIMEOUT);
			ns.Write(User.RoomPos);
			deletedActiveItems.encodeActiveItems(ns);
			_ = last;
		}
	}
}
