using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class LevelUPNotice : NetPacket
	{
		public LevelUPNotice(string lvupstring, byte last)
		{
			ns.WriteOP(Opcodes.eServer_NOTICE_MSG_ACK);
			ns.Write(1);
			ns.Write(3);
			ns.WriteAnsiFixed_intSize(lvupstring);
			_ = last;
		}
	}
}
