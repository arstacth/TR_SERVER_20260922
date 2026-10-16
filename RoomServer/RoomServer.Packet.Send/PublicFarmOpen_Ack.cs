using LocalCommons.Network;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class PublicFarmOpen_Ack : NetPacket
	{
		public PublicFarmOpen_Ack(FarmRoomInfo FarmRoomInfo, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_MODIFY_OPTION_TALKING_ACK);
			ns.Write(0);
			ns.Write(FarmRoomInfo.isPublic);
			if (FarmRoomInfo.isPublic)
			{
				ns.Write(FarmRoomInfo.Type);
				ns.Write(FarmRoomInfo.MaxUserLimit);
				ns.Write(FarmRoomInfo.unk1);
				ns.WriteAnsiFixed_intSize(FarmRoomInfo.FarmName);
				ns.Write((short)0);
			}
			else
			{
				ns.Fill(7);
			}
			_ = last;
		}
	}
}
