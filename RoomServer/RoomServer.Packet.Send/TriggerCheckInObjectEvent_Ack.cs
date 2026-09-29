using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TriggerCheckInObjectEvent_Ack : NetPacket
	{
		public TriggerCheckInObjectEvent_Ack(byte pos, int needcount, int tookcount, int unk2, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_TRIGGER_CHECK_IN_OBJECT_EVENT_ACK);
			ns.Write(pos);
			ns.Write(needcount);
			ns.Write(tookcount);
			ns.Write((byte)0);
			ns.Write(unk2);
			_ = last;
		}
	}
}
