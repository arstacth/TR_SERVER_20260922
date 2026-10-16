using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GameUpdateEXP : NetPacket
	{
		public GameRoom_GameUpdateEXP(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GAME_RESULT_ACK);
			ns.Write(10000);
			ns.Write(User.Exp);
			ns.Write(601);
			ns.Write(0L);
			_ = last;
		}
	}
}
