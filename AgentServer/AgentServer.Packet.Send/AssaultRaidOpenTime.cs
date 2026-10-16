using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AssaultRaidOpenTime : NetPacket
	{
		public AssaultRaidOpenTime(byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_DUNGEON_RAID_SCHEDULE_INFO_ACK);
			ns.Write(2);
			ns.Write(0);
			ns.Write(86400);
			// Extra int left RemainSize=4 — client does not read a 4th int.
		}
	}
}
