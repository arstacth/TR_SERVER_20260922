using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FronTier_SchduleInfo : NetPacket
	{
		public FronTier_SchduleInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_FRONTIER_CCHANNEL_SCHEDULE_INFO_ACK);
			ns.Write(1);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(string.Empty);
			_ = last;
		}
	}
}
