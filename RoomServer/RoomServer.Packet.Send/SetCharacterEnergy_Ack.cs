using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SetCharacterEnergy_Ack : NetPacket
	{
		public SetCharacterEnergy_Ack(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_SET_CHARACTER_ENERGY_INFO_ACK);
			ns.Write(room.PlayerList().Count);
			foreach (Account item in from p in room.PlayerList()
				orderby p.RoomPos
				select p)
			{
				ns.Write((int)item.RoomPos);
				ns.Write(item.HP);
			}
			_ = last;
		}
	}
}
