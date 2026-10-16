using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_PassVertification : NetPacket
	{
		public GameRoom_PassVertification(Account User, int msgid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CONFIRM_NUMBER_FOR_PREVENT_ABUSING_ACK);
			ns.Write(msgid);
			ns.Write(User.VertificationCode);
			ns.Write(User.WrongTime);
			_ = last;
		}
	}
}
