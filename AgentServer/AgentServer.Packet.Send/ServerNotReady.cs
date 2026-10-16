using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ServerNotReady : NetPacket
	{
		public ServerNotReady()
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_FAILED_ACK);
			ns.Write(4);
			ns.Write((byte)1);
		}
	}
}
