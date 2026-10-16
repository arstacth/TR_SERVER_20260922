using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class WeddingDivorceACK : NetPacket
	{
		public WeddingDivorceACK(int DivorceType, bool isEnforce, byte last)
		{
			ns.WriteOP(Opcodes.eServer_WEDDING_DIVORCE_ACK);
			ns.Write(DivorceType);
			ns.Write(isEnforce);
			_ = last;
		}
	}
}
