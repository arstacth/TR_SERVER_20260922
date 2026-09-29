using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_KickPlayer2 : NetPacket
	{
		public GameRoom_KickPlayer2(byte last)
		{
			ns.WriteOP(Opcodes.eServer_DATA_NOTIFIER_COMMAND);
			ns.WriteHex("F703000002000000");
			_ = last;
		}
	}
}
