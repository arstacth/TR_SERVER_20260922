using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AssaultRaidOpenTime : NetPacket
	{
		public AssaultRaidOpenTime(byte last)
		{
			ns.WriteOP(Opcodes.eServer_DUNGEON_RAID_SCHEDULE_INFO_ACK);
			ns.Write(2);
			ns.Write(0);
			ns.Write(86400);
			_ = last;
		}
	}
}
