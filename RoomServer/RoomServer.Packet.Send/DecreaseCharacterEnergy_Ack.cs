using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class DecreaseCharacterEnergy_Ack : NetPacket
	{
		public DecreaseCharacterEnergy_Ack(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_DECREASE_CHARACTER_ENERGY_ACK);
			ns.Write((int)User.RoomPos);
			ns.Write(User.HP);
			_ = last;
		}
	}
}
