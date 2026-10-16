using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ApplyHotTimeEventSuccess : NetPacket
	{
		public ApplyHotTimeEventSuccess(long hottimeid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_HOTTIME_EVENT_APPLY_ACK);
			ns.Write(hottimeid);
			_ = last;
		}
	}
}
