using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket16 : NetPacket
	{
		public UnknownPacket16(byte last)
		{
			ns.WriteOP(Opcodes.eServer_HERO_CLASS_BUFF_BONUS_NOTIFY);
			ns.WriteHex("09000000B613000001000000340800000000000005000000");
			_ = last;
		}
	}
}
