using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_ChangeStatus : NetPacket
	{
		public GameRoom_ChangeStatus(byte pos, int code, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_USER_STATE);
			ns.Write(pos);
			ns.Write(code);
			_ = last;
		}
	}
}
