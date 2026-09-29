using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SaveUserFarmSlotInfoFail_Ack : NetPacket
	{
		public SaveUserFarmSlotInfoFail_Ack(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SaveUserFarmSlotInfo_ACK);
			ns.Write(205);
			ns.Write(9);
			_ = last;
		}
	}
}
