using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse4 : NetPacket
	{
		public LoginUnknownResponse4(byte last)
		{
			ns.WriteOP(Opcodes.eServer_REAL_TIME_ITEMINFO);
			ns.Write(0L);
			_ = last;
		}
	}
}
