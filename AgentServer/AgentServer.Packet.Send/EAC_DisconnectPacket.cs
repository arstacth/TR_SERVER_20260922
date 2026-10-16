using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EAC_DisconnectPacket : NetPacket
	{
		public EAC_DisconnectPacket(string msg, long bantime, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DISCONNECT_FROM_SERVER_ACK);
			ns.Write(9012);
			ns.WriteBIG5Fixed_shortSize(msg);
			ns.Write(bantime);
			_ = last;
		}
	}
}
