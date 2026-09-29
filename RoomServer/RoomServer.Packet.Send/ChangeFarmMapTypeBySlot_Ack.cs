using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChangeFarmMapTypeBySlot_Ack : NetPacket
	{
		public ChangeFarmMapTypeBySlot_Ack(int FarmUniqueNum, NormalRoom room, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmMapTypeBySlot_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(room.FarmRoomInfo.FarmTypeNum);
			ns.Write((byte)1);
			ns.Write(room.FarmRoomInfo.FarmSkyTypeNum);
			ns.Write(room.FarmRoomInfo.FarmWeatherTypeNum);
			ns.Write(0);
			_ = last;
		}
	}
}
