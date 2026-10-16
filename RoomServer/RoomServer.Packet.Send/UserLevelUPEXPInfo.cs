using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class UserLevelUPEXPInfo : NetPacket
	{
		public UserLevelUPEXPInfo(short type, int level, long expvalue, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LEVEL_UP_ME_ACK);
			ns.Write(type);
			ns.Write(level);
			ns.Write(expvalue);
			_ = last;
		}
	}
}
