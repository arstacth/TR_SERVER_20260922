using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TriggerObjectEvent_Ack : NetPacket
	{
		public TriggerObjectEvent_Ack(int unk, byte isLeave, int unk3, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_OBJECT_EVENT_ACK);
			ns.Write(unk);
			ns.Write(isLeave);
			ns.Write(unk3);
			_ = last;
		}
	}
}
