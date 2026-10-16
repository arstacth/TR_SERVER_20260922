using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_WaitPassBaton : NetPacket
	{
		public GameRoom_WaitPassBaton(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RELAY_PREPARE_NEXT_RUNNER_ACK);
			ns.Write(User.RoomPos);
			_ = last;
		}
	}
}
