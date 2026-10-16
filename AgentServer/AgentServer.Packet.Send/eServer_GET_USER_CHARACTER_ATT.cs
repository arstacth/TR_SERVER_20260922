using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_USER_CHARACTER_ATTR_ACK : NetPacket
	{
		public eServer_GET_USER_CHARACTER_ATTR_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_CHARACTER_ATTR_ACK);
			ns.Write(0);
			User.encodeUserCharAttr(ns);
			// Thai RemainSize=1 when last was written.
			_ = last;
		}
	}
}
