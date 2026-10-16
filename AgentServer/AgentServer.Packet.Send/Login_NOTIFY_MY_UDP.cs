using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Login_NOTIFY_MY_UDP : NetPacket
	{
		public Login_NOTIFY_MY_UDP(byte last)
		{
			ns.WriteOP(Opcodes.eServer_NOTIFY_MY_UDP_ACK);
		}
	}
}
