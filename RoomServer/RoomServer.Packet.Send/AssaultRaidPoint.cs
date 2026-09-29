using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AssaultRaidPoint : NetPacket
	{
		public AssaultRaidPoint(int point, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DUNGEON_RAID_GET_MY_POINT_ACK);
			ns.Write(point);
			_ = last;
		}
	}
}
