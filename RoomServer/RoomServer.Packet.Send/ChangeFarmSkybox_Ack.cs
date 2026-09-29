using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChangeFarmSkybox_Ack : NetPacket
	{
		public ChangeFarmSkybox_Ack(int FarmUniqueNum, NormalRoom room, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmSkybox_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(room.FarmRoomInfo.FarmSkyTypeNum);
			_ = last;
		}
	}
}
