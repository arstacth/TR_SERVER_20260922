using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AssaultModeRebirth_Ack : NetPacket
	{
		public AssaultModeRebirth_Ack(Account User, bool isRealRebirth, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_REBIRTH_ACK);
			ns.Write((int)User.RoomPos);
			ns.Write(User.HP);
			ns.Write(isRealRebirth);
			_ = last;
		}
	}
}
