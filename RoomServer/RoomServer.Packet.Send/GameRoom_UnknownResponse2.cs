using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_UnknownResponse2 : NetPacket
	{
		public GameRoom_UnknownResponse2(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GUILD_BUF_GUILD_NUM_LIST_NOTIFY);
			ns.Write(0);
			_ = last;
		}
	}
}
