using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_TimeInfo_ACK : NetPacket
	{
		public TowerEvent_TimeInfo_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_OPENINFO_ACK);
			ns.Write(ServerStatus.TowerEventEnable);
			ns.Write(ServerStatus.TowerEventEnable ? 1 : 0);
			ns.Write(ServerStatus.TowerEventEnable ? Utility.ConvertToTimestamp(ServerStatus.TowerEventStartTime) : 0);
			ns.Write(ServerStatus.TowerEventEnable ? Utility.ConvertToTimestamp(ServerStatus.TowerEventEndTime) : 0);
			ns.Write(ServerStatus.TowerEventEnable ? Utility.ConvertToTimestamp(ServerStatus.TowerEventStartTime.AddHours(1.0)) : 0);
			_ = last;
		}
	}
}
