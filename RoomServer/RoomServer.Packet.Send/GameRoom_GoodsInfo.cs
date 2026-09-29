using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GoodsInfo : NetPacket
	{
		public GameRoom_GoodsInfo(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_REGISTER_REWARD_NOTIFY);
			ns.Write(room.ItemNum);
			ns.Write(0);
			ns.Write(room.isOrderBy);
			ns.Write(room.SendRank);
			ns.Write(room.isPublic);
			_ = last;
		}
	}
}
