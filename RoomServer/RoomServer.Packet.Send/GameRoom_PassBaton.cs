using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_PassBaton : NetPacket
	{
		public GameRoom_PassBaton(Account User, byte teampos, int unk, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BATON_INPUT_ACK);
			ns.Write(User.RoomPos);
			ns.Write(teampos);
			ns.Write(unk);
			_ = last;
		}
	}
}
