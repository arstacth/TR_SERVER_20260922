using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HotTimeEventDelete : NetPacket
	{
		public HotTimeEventDelete(long hottimeid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_HOTTIME_EVENT_DELETE_ACK);
			ns.Write(hottimeid);
			_ = last;
		}
	}
}
