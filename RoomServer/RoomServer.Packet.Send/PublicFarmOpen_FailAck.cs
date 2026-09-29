using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class PublicFarmOpen_FailAck : NetPacket
	{
		public PublicFarmOpen_FailAck(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_MODIFY_OPTION_TALKING_ACK);
			ns.Write(218);
			_ = last;
		}
	}
}
