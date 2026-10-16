using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RegisterSuccess : NetPacket
	{
		public GameRoom_RegisterSuccess(int itemnum, long storage_id, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MASTER_REGISTER_REWARD_ACK);
			ns.Write(0);
			ns.Write(itemnum);
			ns.Write(storage_id);
			ns.Write(0);
			_ = last;
		}
	}
}
