using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CheckDivinationFree_ACK : NetPacket
	{
		public CheckDivinationFree_ACK(int pDivination, bool isFree, byte last)
		{
			ns.WriteOP(Opcodes.eServer_Divination_CHECK_FREE_ASK);
			ns.Write(0);
			ns.Write(pDivination);
			ns.Write(isFree);
			_ = last;
		}
	}
}
