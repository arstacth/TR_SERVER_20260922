using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChangeFarmName_Ack : NetPacket
	{
		public ChangeFarmName_Ack(int FarmUniqueNum, string name, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_CHANGE_FARM_NAME_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
