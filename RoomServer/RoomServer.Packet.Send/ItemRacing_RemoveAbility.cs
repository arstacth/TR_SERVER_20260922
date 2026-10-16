using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ItemRacing_RemoveAbility : NetPacket
	{
		public ItemRacing_RemoveAbility(Account User, int group, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_REMOVE_ACK);
			ns.Write(group);
			ns.Write((int)User.RoomPos);
			ns.Write(0);
			_ = last;
		}
	}
}
