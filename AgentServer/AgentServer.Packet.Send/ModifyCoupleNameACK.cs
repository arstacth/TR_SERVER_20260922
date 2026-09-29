using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyCoupleNameACK : NetPacket
	{
		public ModifyCoupleNameACK(string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_MODIFY_COUPLE_NAME_ACK);
			ns.Write((byte)0);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
