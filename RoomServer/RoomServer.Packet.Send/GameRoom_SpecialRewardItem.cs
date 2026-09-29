using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_SpecialRewardItem : NetPacket
	{
		public GameRoom_SpecialRewardItem(int itemid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SPECIAL_GAME_REWARD_NOTIFY);
			ns.WriteHex("01000000006464000000");
			ns.Write(itemid);
			ns.Write(1);
			_ = last;
		}
	}
}
