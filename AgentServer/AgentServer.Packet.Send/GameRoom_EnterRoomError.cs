using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameRoom_EnterRoomError : NetPacket
	{
		public GameRoom_EnterRoomError(int errorid, int roomkindid, byte last, int value = 0)
		{
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_ACK);
			ns.Write(67);
			_ = last;
		}
	}
}
