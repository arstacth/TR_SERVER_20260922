using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetExchangeSystemOK : NetPacket
	{
		public GetExchangeSystemOK(Account User, int SystemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EXCHANGE_SYSTEM_GET_USE_INFO_ACK);
			ns.Write(SystemNum);
			ns.Write(0);
			_ = last;
		}
	}
}
