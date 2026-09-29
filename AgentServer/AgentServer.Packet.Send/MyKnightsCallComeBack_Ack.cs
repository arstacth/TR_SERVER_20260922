using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyKnightsCallComeBack_Ack : NetPacket
	{
		public MyKnightsCallComeBack_Ack(int CompleteGroupNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_PVE_COMEBACK_ACK);
			ns.Write(CompleteGroupNum);
			_ = last;
		}
	}
}
