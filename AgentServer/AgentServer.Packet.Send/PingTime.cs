using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PingTime : NetPacket
	{
		public PingTime(bool isLogin, long time, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_SERVER_TIME_ACK);
			ns.Write(time);
			_ = last;
		}
	}
}
