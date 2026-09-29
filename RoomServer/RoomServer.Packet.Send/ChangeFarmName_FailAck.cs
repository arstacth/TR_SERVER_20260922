using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChangeFarmName_FailAck : NetPacket
	{
		public ChangeFarmName_FailAck(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_CHANGE_FARM_NAME_ACK);
			ns.Write(214);
			ns.Write((byte)5);
			_ = last;
		}
	}
}
