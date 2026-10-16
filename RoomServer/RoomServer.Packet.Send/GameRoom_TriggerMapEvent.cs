using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_TriggerMapEvent : NetPacket
	{
		public GameRoom_TriggerMapEvent(byte eventnum, int eventlaptime, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_MAP_EVENT_ACK);
			ns.Write(eventnum);
			ns.Write(eventlaptime);
			_ = last;
		}
	}
}
