using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_UnknownResponse : NetPacket
	{
		public GameRoom_UnknownResponse(byte last)
		{
			ns.WriteOP(Opcodes.eServer_DATA_NOTIFIER_COMMAND);
			ns.WriteHex("F70300000000000000000000");
			_ = last;
		}
	}
}
