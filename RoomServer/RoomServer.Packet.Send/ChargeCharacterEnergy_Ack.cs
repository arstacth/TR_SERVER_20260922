using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChargeCharacterEnergy_Ack : NetPacket
	{
		public ChargeCharacterEnergy_Ack(Account User, bool isFullAlready, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_CHARGE_CHARACTER_ENERGY_ACK);
			ns.Write((int)User.RoomPos);
			ns.Write((!isFullAlready) ? 1 : 0);
			ns.Write(User.HP);
			_ = last;
		}
	}
}
