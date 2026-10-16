using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SinglePlayGoal_ACK : NetPacket
	{
		public SinglePlayGoal_ACK(int ms, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALONERUN_GOAL_IN_NORMAL_END_ACK);
			ns.Write(ms);
			_ = last;
		}
	}
}
