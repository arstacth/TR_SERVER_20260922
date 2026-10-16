using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TcpLiveMsgAck : NetPacket
	{
		public TcpLiveMsgAck()
		{
			ns.WriteOP(Opcodes.eServer_LIVE_MSG);
		}
	}
}
