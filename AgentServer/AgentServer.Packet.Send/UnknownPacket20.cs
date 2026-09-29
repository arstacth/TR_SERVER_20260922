using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket20 : NetPacket
	{
		public UnknownPacket20(byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_DUELINFO_PVP_ACK);
			ns.Fill(20);
			_ = last;
		}
	}
}
