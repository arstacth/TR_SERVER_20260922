using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_SERVER_TIME_ACK : NetPacket
	{
		public eServer_GET_SERVER_TIME_ACK()
		{
			ns.WriteOP(Opcodes.eServer_GET_SERVER_TIME_ACK);
			ns.Write(Utility.CurrentTimeMilliseconds());
		}
	}
}
