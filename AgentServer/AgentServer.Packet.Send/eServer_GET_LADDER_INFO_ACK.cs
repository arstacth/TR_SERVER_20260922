using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_LADDER_INFO_ACK : NetPacket
	{
		public eServer_GET_LADDER_INFO_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_LADDER_INFO);
			ns.Write(10000);
			ns.Write(-1);
			_ = last;
		}
	}
}
