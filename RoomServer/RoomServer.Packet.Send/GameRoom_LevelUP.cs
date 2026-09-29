using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_LevelUP : NetPacket
	{
		public GameRoom_LevelUP(short type, long exp, byte pos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LEVEL_UP_ACK);
			ns.Write(type);
			ns.Write(exp);
			ns.Write(pos);
			_ = last;
		}
	}
}
