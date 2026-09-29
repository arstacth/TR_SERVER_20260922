using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket15 : NetPacket
	{
		public UnknownPacket15(string value, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DRAGON_ALCHEMIST_UNIT_BONUS_NOTIFY);
			ns.WriteHex(value);
			_ = last;
		}
	}
}
