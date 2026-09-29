using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class BonusStage_ExtraPoint : NetPacket
	{
		public BonusStage_ExtraPoint(int point, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BONUS_STAGE_LAST_CHANCE_POINT_ACK);
			ns.Write(point);
			ns.Write(0);
			_ = last;
		}
	}
}
