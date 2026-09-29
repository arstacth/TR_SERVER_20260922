using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse3 : NetPacket
	{
		public LoginUnknownResponse3(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_MEMBERSHIPITEM_INFO);
			ns.Write(0);
			_ = last;
		}
	}
}
