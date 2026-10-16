using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GetVertificationInfo : NetPacket
	{
		public GameRoom_GetVertificationInfo(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_NUMBER_FOR_PREVENT_ABUSING_ACK);
			ns.Write(User.VertificationCode);
			_ = last;
		}
	}
}
