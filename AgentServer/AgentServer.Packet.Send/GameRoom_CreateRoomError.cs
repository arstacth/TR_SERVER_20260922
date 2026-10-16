using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameRoom_CreateRoomError : NetPacket
	{
		public GameRoom_CreateRoomError(int errorid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MAKE_ROOM_FAILED_ACK);
			ns.Write(errorid);
			_ = last;
		}
	}
}
